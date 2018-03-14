using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ISimulator
    {
        ObjectId BeginSimulation();

        void RunForTicks(ObjectId simulationId, int ticks);

        SimulationView GetLatest(ObjectId simulationId);

        SimulationView GetAtTick(ObjectId simulationId, int tick);
    }
}