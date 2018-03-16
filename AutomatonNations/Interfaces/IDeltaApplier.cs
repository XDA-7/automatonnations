using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IDeltaApplier
    {
        SimulationView GetForTick(ObjectId simulationId, int tick);
    }
}