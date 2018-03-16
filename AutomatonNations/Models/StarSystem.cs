using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class StarSystem
    {
        public ObjectId Id { get; set; }

        public Coordinate Coordinate { get; set; }

        public double Development { get; set; }

        public IEnumerable<ObjectId> ConnectedSystemIds { get; set; }
    }
}