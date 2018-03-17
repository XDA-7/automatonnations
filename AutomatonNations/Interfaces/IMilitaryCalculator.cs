namespace AutomatonNations
{
    public interface IMilitaryCalculator
    {
        int ProductionForEmpire(EmpireSystemsView empire);

        CombatResult Combat(Empire attacker, Empire defender);
    }
}