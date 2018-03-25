using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IMilitarySimulator
    {
        void Run(DeltaMetadata deltaMetadata, ObjectId simulationId);
    }
}