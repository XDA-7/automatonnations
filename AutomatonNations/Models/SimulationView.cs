using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class SimulationView
    {
        public Simulation Simulation { get; set; }

        public IEnumerable<StarSystem> StarSystems { get; set; }

        public IEnumerable<Empire> Empires { get; set; }

        public IEnumerable<War> Wars { get; set; }
    }
}