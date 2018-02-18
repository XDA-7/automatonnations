using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class StarSystem
    {
        public ObjectId Id { get; set; }

        public Coordinate Coordinate { get; set; }

        public int Development { get; set; }

        public int DistanceToCapital { get; set; }

        public IEnumerable<ObjectId> ConnectedSystems { get; set; }
    }
}