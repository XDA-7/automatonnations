using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class Empire
    {
        public ObjectId Id { get; set; }
        
        public IEnumerable<ObjectId> StarSystemsIds { get; set; }

        public Alignment Alignment { get; set; }

        public double Military { get; set; }
    }
}