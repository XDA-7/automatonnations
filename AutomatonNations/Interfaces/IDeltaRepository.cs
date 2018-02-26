using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IDeltaRepository
    {
        DeltaSet GetForSimulation(ObjectId simulationId, int startTick, int endTick);
    }
}