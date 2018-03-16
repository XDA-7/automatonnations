using MongoDB.Bson;

namespace AutomatonNations
{
    public class GrowthFromSystemResult
    {
        public ObjectId SystemId { get; set; }

        public double Growth { get; set; }

        public GrowthFromSystemResult(ObjectId systemId, double growth)
        {
            SystemId = systemId;
            Growth = growth;
        }
    }
}