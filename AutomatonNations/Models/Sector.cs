using MongoDB.Bson;

namespace AutomatonNations
{
    public class Sector
    {
        public ObjectId Id { get; set; }

        public ObjectId[] StarSystems { get; set; }
    }
}