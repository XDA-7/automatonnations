using MongoDB.Bson;

namespace AutomatonNations
{
    public class DecimalDelta
    {
        public DeltaType DeltaType { get; set; }
        
        public ObjectId Id { get; set; }

        public decimal Value { get; set; }
    }
}