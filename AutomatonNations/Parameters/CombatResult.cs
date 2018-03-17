namespace AutomatonNations
{
    public class CombatResult
    {
        public Damage AttackerDamage { get; }

        public Damage DefenderDamage { get; }

        public TerritoryGain TerritoryGain { get; }

        public CombatResult(
            int attackerMilitaryDamage,
            int attackerCollateralDamage,
            int defenderMilitaryDamage,
            int defenderCollateralDamage,
            TerritoryGain territoryGain)
        {
            AttackerDamage = new Damage(attackerMilitaryDamage, attackerCollateralDamage);
            DefenderDamage = new Damage(defenderMilitaryDamage, defenderCollateralDamage);
            TerritoryGain = territoryGain;
        }
    }

    public class Damage
    {
        public int MilitaryDamage { get; }

        public int CollateralDamage { get; }

        public Damage(int militaryDamage, int collateralDamage)
        {
            MilitaryDamage = militaryDamage;
            CollateralDamage = collateralDamage;
        }
    }
}