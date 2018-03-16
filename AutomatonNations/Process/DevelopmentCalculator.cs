using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class DevelopmentCalculator : IDevelopmentCalculator
    {
        public IEnumerable<GrowthFromSystemResult> GrowthFromSystem(StarSystem system, IEnumerable<StarSystem> connectedSystems, double growthFocus)
        {
            var income = system.Development * Parameters.IncomeRate * growthFocus;
            if (income == 0)
            {
                return new GrowthFromSystemResult[0];
            }

            var totalDevelopment = system.Development + connectedSystems.Sum(x => x.Development);
            return connectedSystems
                .Concat(new StarSystem[] { system })
                .Select(x => new GrowthFromSystemResult(x.Id, income * (x.Development / totalDevelopment)));
        }
    }
}