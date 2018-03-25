using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IDiplomacySimulator
    {
        void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId);
    }
}