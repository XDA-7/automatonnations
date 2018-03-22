namespace AutomatonNations
{
    public interface IMilitaryCalculator
    {
        double ProductionForEmpire(EmpireSystemsView empire);

        CombatResult Combat(Empire attacker, Empire defender);
    }
}