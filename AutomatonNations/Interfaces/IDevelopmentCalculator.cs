using System.Collections.Generic;

namespace AutomatonNations
{
    public interface IDevelopmentCalculator
    {
        IEnumerable<GrowthFromSystemResult> GrowthFromSystem(StarSystem system, IEnumerable<StarSystem> connectedSystems, double growthFocus);
    }
}