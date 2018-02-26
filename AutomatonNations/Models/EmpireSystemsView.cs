using System.Collections.Generic;

namespace AutomatonNations
{
    public class EmpireSystemsView
    {
        public Empire Empire { get; set; }

        public IEnumerable<StarSystem> StarSystems { get; set; }
    }
}