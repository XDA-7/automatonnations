using System.Collections.Generic;

namespace AutomatonNations
{
    public interface IDevelopmentCalculator
    {
        IEnumerable<DecimalDelta> GrowthFromSystem(StarSystem system, IEnumerable<StarSystem> connectedSystems);
    }
}