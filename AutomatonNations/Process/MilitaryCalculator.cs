using System.Linq;

namespace AutomatonNations
{
    public class MilitaryCalculator : IMilitaryCalculator
    {
        private IRandom _random;

        public MilitaryCalculator(IRandom random)
        {
            _random = random;
        }

        public double ProductionForEmpire(EmpireSystemsView empire)
        {
            var totalDevelopment = empire.StarSystems.Sum(x => x.Development);
            return totalDevelopment * empire.Empire.Alignment.Power;
        }

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
    }
}