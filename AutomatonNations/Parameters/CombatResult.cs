namespace AutomatonNations
{
    public class CombatResult
    {
        public Damage AttackerDamage { get; }

        public Damage DefenderDamage { get; }

        public TerritoryGain TerritoryGain { get; }

        public CombatResult(
            double attackerMilitaryDamage,
            double attackerCollateralDamage,
            double defenderMilitaryDamage,
            double defenderCollateralDamage,
            TerritoryGain territoryGain)
        {
            AttackerDamage = new Damage(attackerMilitaryDamage, attackerCollateralDamage);
            DefenderDamage = new Damage(defenderMilitaryDamage, defenderCollateralDamage);
            TerritoryGain = territoryGain;
        }
    }

    public class Damage
    {
        public double MilitaryDamage { get; }

        public double CollateralDamage { get; }

        public Damage(double militaryDamage, double collateralDamage)
        {
            MilitaryDamage = militaryDamage;
            CollateralDamage = collateralDamage;
        }
    }
}