using MongoDB.Bson;

namespace AutomatonNations
{
    public class Delta
    {
        public ObjectId Id { get; set; }

        public DeltaType DeltaType { get; set; }

        public int Tick { get; set; }
        
        public ObjectId ReferenceId { get; set; }

        public ObjectId SimulationId { get; set; }
    }

    public class Delta<T>
    {
        public ObjectId Id { get; set; }

        public DeltaType DeltaType { get; set; }

        public int Tick { get; set; }

        public T Value { get; set; }
        
        public ObjectId ReferenceId { get; set; }

        public ObjectId SimulationId { get; set; }
    }
}