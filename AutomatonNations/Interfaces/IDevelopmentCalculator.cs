using System.Collections.Generic;

namespace AutomatonNations
{
    public interface IDevelopmentCalculator
    {
        IEnumerable<Delta<decimal>> GrowthFromSystem(StarSystem system, IEnumerable<StarSystem> connectedSystems, decimal growthFocus);
    }
}