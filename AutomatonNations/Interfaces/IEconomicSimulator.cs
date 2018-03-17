namespace AutomatonNations
{
    public interface IEconomicSimulator
    {
        void RunEmpire(DeltaMetadata deltaMetadata, EmpireSystemsView empire);

        void ApplyDamage(DeltaMetadata deltaMetadata, EmpireBorderView empireBorderView, double empireDamage, double borderingEmpireDamage);
    }
}