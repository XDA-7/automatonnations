using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations.Presentation
{
    public class StarSystem
    {
        public Coordinate Coordinate { get; set; }

        public double Development { get; set; }

        public IEnumerable<ObjectId> EmpireIds { get; set; }
    }
}