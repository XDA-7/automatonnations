using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class Simulation
    {
        public ObjectId Id { get; set; }
        
        public int Ticks { get; set; }

        public IEnumerable<ObjectId> EmpireIds { get; set; }

        public IEnumerable<ObjectId> WarIds { get; set; }

        public ObjectId SectorId { get; set; }
    }
}