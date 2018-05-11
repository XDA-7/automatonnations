using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class MilitaryCalculator : IMilitaryCalculator
    {
        private ProductionForEmpireDelegate _productionForEmpire;
        private IRandom _random;

        public MilitaryCalculator(IConfiguration configuration, IRandom random)
        {
            if (configuration.CapMilitaryProduction)
            {
                _productionForEmpire = ProductionForEmpireCapped;
            }
            else
            {
                _productionForEmpire = ProductionForEmpireUncapped;
            }
            
            _random = random;
        }

        public MilitaryProductionResult ProductionForEmpire(EmpireSystemsView empire) => _productionForEmpire(empire);

        public CombatResult Combat(Empire attacker, Empire defender)
        {
            var damageMultiplers = _random.DoubleSet(
                minVal: Parameters.MilitaryDamageRateMinimum,
                maxVal: Parameters.MilitaryDamageRateMaximum,
                count: 2);
            var attackerDamage = CalculateDamage(attacker.Military, damageMultiplers[0]);
            var defenderDamage = CalculateDamage(defender.Military, damageMultiplers[1]);
            var territoryGain = CalculateTerritoryGain(attackerDamage.MilitaryDamage, attacker.Military, defenderDamage.MilitaryDamage, defender.Military);
            return new CombatResult(
                attackerDamage.MilitaryDamage,
                attackerDamage.CollateralDamage,
                defenderDamage.MilitaryDamage,
                defenderDamage.CollateralDamage,
                territoryGain);
        }

        public double EmpireTotalMilitary(Empire empire) =>
            empire.Military + empire.Leaders.Select(leader => leader.Military).Sum();

        public EmpireMilitaryDamageResult EmpireMilitaryDamageDistribution(Empire empire, double damage)
        {
            var totalMilitary = EmpireTotalMilitary(empire);
            var empireDirectProportion = empire.Military / totalMilitary;
            var empireDamage = empireDirectProportion * damage;
            var leaderDamage = damage - empireDamage;
            return new EmpireMilitaryDamageResult(
                empireDamage,
                GetMilitaryDamageUpdatedLeaders(empire.Leaders, leaderDamage));
        }

        private MilitaryProductionResult ProductionForEmpireCapped(EmpireSystemsView empire)
        {
            var militaryProduction = GetSystemMilitaryProduction(empire);
            var updatedLeaders = GetMilitaryProductionUpdatedLeaders(militaryProduction, empire.Empire.Leaders);
            CapLeaderMilitary(updatedLeaders, empire.StarSystems);
            var empireProduction = militaryProduction.Values.Select(x => x.MilitaryProduction).Sum();
            var expansionCapacity = GetEmpireExpansionCapacity(empire);
            empireProduction = Math.Min(empireProduction, expansionCapacity);
            return new MilitaryProductionResult(empireProduction, updatedLeaders);
        }

        private double GetEmpireExpansionCapacity(EmpireSystemsView empire)
        {
            var totalDevelopment = empire.StarSystems.Sum(x => x.Development);
            return (totalDevelopment * Parameters.MilitaryCapDevelopmentProportion) - empire.Empire.Military;
        }

        private MilitaryProductionResult ProductionForEmpireUncapped(EmpireSystemsView empire)
        {
            var militaryProduction = GetSystemMilitaryProduction(empire);
            var updatedLeaders = GetMilitaryProductionUpdatedLeaders(militaryProduction, empire.Empire.Leaders);
            var empireProduction = militaryProduction.Values.Select(x => x.MilitaryProduction).Sum();
            return new MilitaryProductionResult(empireProduction, updatedLeaders);
        }

        private Damage CalculateDamage(double military, double multiplier)
        {
            var militaryDamage = military * multiplier;
            var collateralDamage = militaryDamage * Parameters.CollateralDamageRate;
            return new Damage(militaryDamage, collateralDamage);
        }

        private TerritoryGain CalculateTerritoryGain(double attackerDamage, double attackerMilitary, double defenderDamage, double defenderMilitary)
        {
            var attackerAdvantage = attackerDamage - defenderDamage;
            if (attackerAdvantage > 0 && IsAboveThreshold(attackerAdvantage, defenderMilitary))
            {
                return TerritoryGain.Attacker;
            }
            else if (attackerAdvantage < 0 && IsAboveThreshold(-attackerAdvantage, attackerMilitary))
            {
                return TerritoryGain.Defender;
            }
            else
            {
                return TerritoryGain.None;
            }
        }

        private Dictionary<ObjectId, SystemMilitaryProduction> GetSystemMilitaryProduction(EmpireSystemsView empire) =>
            empire.StarSystems
                .Select(system => new SystemMilitaryProduction(system.Id, system.Development * empire.Empire.Alignment.Power))
                .ToDictionary(x => x.SystemId);

        private Leader[] GetMilitaryDamageUpdatedLeaders(IEnumerable<Leader> leaders, double totalDamage)
        {
            var totalMilitary = leaders.Select(leader => leader.Military).Sum();
            var result = new List<Leader>();
            foreach (var leader in leaders)
            {
                var damage = totalDamage * (leader.Military / totalMilitary);
                leader.Military -= damage;
                result.Add(leader);
            }

            return result.ToArray();
        }

        private Leader[] GetMilitaryProductionUpdatedLeaders(Dictionary<ObjectId, SystemMilitaryProduction> systems, IEnumerable<Leader> leaders)
        {
            var result = new List<Leader>();
            foreach (var leader in leaders)
            {
                foreach (var leaderSystemId in leader.StarSystemIds)
                {
                    var leaderSystem = systems[leaderSystemId];
                    var witheldMilitary = leaderSystem.MilitaryProduction * leader.MilitaryWitholdingRate;
                    leader.Military += witheldMilitary;
                    leaderSystem.MilitaryProduction -= witheldMilitary;
                }

                result.Add(leader);
            }

            return result.ToArray();
        }

        private void CapLeaderMilitary(Leader[] leaders, IEnumerable<StarSystem> starSystems)
        {
            foreach (var leader in leaders)
            {
                var leaderDevelopment = starSystems
                    .Where(system => leader.StarSystemIds.Contains(system.Id))
                    .Sum(system => system.Development);
                var militaryCap = leaderDevelopment * Parameters.MilitaryCapDevelopmentProportion;
                leader.Military = Math.Min(militaryCap, leader.Military);
            }
        }

        private bool IsAboveThreshold(double advantage, double opposingForce) =>
            (advantage / opposingForce) > Parameters.MilitaryAdvantageLineAdvanceThreshold;

        private delegate MilitaryProductionResult ProductionForEmpireDelegate(EmpireSystemsView empire);

        private class SystemMilitaryProduction
        {
            public ObjectId SystemId { get; }

            public double MilitaryProduction { get; set; }

            public SystemMilitaryProduction(ObjectId systemId, double militaryProduction)
            {
                SystemId = systemId;
                MilitaryProduction = militaryProduction;
            }
        }
    }
}