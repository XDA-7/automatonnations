using System.Collections.Generic;

namespace AutomatonNations.Presentation
{
    public class Sector
    {
        public int Tick { get; set; }
        
        public IEnumerable<StarSystem> StarSystems { get; set; }

        public IEnumerable<StarSystemConnection> StarSystemConnections { get; set; }
    }
}