using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IDeltaApplier
    {
        SimulationView Apply(ObjectId simulationId, int backTicks);
    }
}