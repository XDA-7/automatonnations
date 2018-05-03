using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ILeaderSimulator
    {
        void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId);
    }
}