namespace AutomatonNations
{
    public interface IEconomicSimulator
    {
        void RunEmpire(DeltaMetadata deltaMetadata, EmpireSystemsView empire);

        void ApplyDamage(EmpireBorderView empireBorderView, int empireDamage, int borderingEmpireDamage);
    }
}