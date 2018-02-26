using System.Collections.Generic;

namespace AutomatonNations
{
    public class ConnectedSystemsView
    {
        public StarSystem StarSystem { get; set; }
        
        public IEnumerable<StarSystem> ConnectedSystems { get; set; }
    }
}