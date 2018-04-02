using System.Linq;

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

        public double ProductionForEmpire(EmpireSystemsView empire) => _productionForEmpire(empire);

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

        private double ProductionForEmpireCapped(EmpireSystemsView empire)
        {
            var totalDevelopment = empire.StarSystems.Sum(x => x.Development);
            var expansionCapacity = (totalDevelopment * Parameters.MilitaryCapDevelopmentProportion) - empire.Empire.Military;
            var expansion = totalDevelopment * empire.Empire.Alignment.Power;
            if (expansion > expansionCapacity)
            {
                return expansionCapacity;
            }
            else
            {
                return expansion;
            }
        }

        private double ProductionForEmpireUncapped(EmpireSystemsView empire)
        {
            var totalDevelopment = empire.StarSystems.Sum(x => x.Development);
            return totalDevelopment * empire.Empire.Alignment.Power;
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

        private bool IsAboveThreshold(double advantage, double opposingForce) =>
            (advantage / opposingForce) > Parameters.MilitaryAdvantageLineAdvanceThreshold;

        private delegate double ProductionForEmpireDelegate(EmpireSystemsView empire);
    }
}