namespace AutomatonNations
{
    public interface IMilitaryCalculator
    {
        MilitaryProductionResult ProductionForEmpire(EmpireSystemsView empire);

        CombatResult Combat(Empire attacker, Empire defender);
    }
}