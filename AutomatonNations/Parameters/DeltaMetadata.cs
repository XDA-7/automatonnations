using MongoDB.Bson;

namespace AutomatonNations
{
    public class DeltaMetadata
    {
        public ObjectId SimulationId { get; }

        public int Tick { get; }

        public DeltaMetadata(ObjectId simulationId, int tick)
        {
            SimulationId = simulationId;
            Tick = tick;
        }
    }
}