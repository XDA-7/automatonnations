using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class Sector
    {
        public ObjectId Id { get; set; }

        public IEnumerable<ObjectId> StarSystemIds { get; set; }
    }
}