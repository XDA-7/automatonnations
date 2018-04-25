using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IDisplayGenerator
    {
        void CreateForSimulation(ObjectId simulationId);

        void CreateForSimulation(ObjectId simulationId, int startTick, int endTick);
    }
}