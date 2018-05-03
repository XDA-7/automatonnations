using MongoDB.Bson;

namespace AutomatonNations
{
    public class MilitaryLevy
    {
        public ObjectId Id { get; set; }

        public ObjectId EmpireId { get; set; }

        public ObjectId LeaderId { get; set; }

        public int Size { get; set; }
    }
}