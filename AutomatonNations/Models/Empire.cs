using MongoDB.Bson;

namespace AutomatonNations
{
    public class Empire
    {
        public ObjectId Id { get; set; }
        
        public ObjectId[] Systems { get; set; }

        public Alignment Alignment { get; set; }
    }
}