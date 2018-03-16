using MongoDB.Bson;

namespace AutomatonNations
{
    public class DeltaMetadata
    {
        public ObjectId SimulationId { get; set; }

        public int Tick { get; set; }

        public DeltaMetadata(ObjectId simulationId, int tick)
        {
            SimulationId = simulationId;
            Tick = tick;
        }
    }
}