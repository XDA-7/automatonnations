namespace AutomatonNations
{
    public interface IMilitaryCalculator
    {
        MilitaryProductionResult ProductionForEmpire(EmpireSystemsView empire);

        CombatResult Combat(Empire attacker, Empire defender);

        double EmpireTotalMilitary(Empire empire);

        EmpireMilitaryDamageResult EmpireMilitaryDamageDistribution(Empire empire, double damage);
    }
}