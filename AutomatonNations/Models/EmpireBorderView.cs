using System.Collections.Generic;

namespace AutomatonNations
{
    public class EmpireBorderView
    {
        public Empire Empire { get; set; }

        public Empire BorderingEmpire { get; set; }

        public IEnumerable<StarSystem> EmpireSystems { get; set; }

        public IEnumerable<StarSystem> BorderingEmpireSystems { get; set; }
    }
}