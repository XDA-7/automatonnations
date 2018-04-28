using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations.Presentation
{
    public class Sector
    {
        public ObjectId SimulationId { get; set; }

        public int Tick { get; set; }
        
        public IEnumerable<StarSystem> StarSystems { get; set; }

        public IEnumerable<StarSystemConnection> StarSystemConnections { get; set; }
    }
}