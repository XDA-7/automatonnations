using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IEconomicSimulator
    {
        void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId);

        void ApplyDamage(DeltaMetadata deltaMetadata, EmpireBorderView empireBorderView, double empireDamage, double borderingEmpireDamage);
    }
}