using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class SimulationView
    {
        public int Ticks { get; set; }

        public IEnumerable<StarSystem> StarSystems { get; set; }

        public IEnumerable<Empire> Empires { get; set; }
    }
}