using MongoDB.Bson;

namespace AutomatonNations
{
    public class GrowthFromSystemResult
    {
        public ObjectId SystemId { get; }

        public double Growth { get; }

        public GrowthFromSystemResult(ObjectId systemId, double growth)
        {
            SystemId = systemId;
            Growth = growth;
        }
    }
}