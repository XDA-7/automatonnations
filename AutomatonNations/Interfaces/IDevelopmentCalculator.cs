using System.Collections.Generic;

namespace AutomatonNations
{
    public interface IDevelopmentCalculator
    {
        IEnumerable<GrowthFromSystemResult> GrowthFromSystem(StarSystem starSystem, EmpireSystemsView empireView);
    }
}