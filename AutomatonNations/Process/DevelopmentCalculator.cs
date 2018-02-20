using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class DevelopmentCalculator : IDevelopmentCalculator
    {
        public IEnumerable<DecimalDelta> GrowthFromSystem(StarSystem system, IEnumerable<StarSystem> connectedSystems)
        {
            var income = system.Development * Parameters.IncomeRate;
            if (income == 0)
            {
                return new DecimalDelta[0];
            }

            var totalDevelopment = system.Development + connectedSystems.Sum(x => x.Development);
            return connectedSystems
                .Concat(new StarSystem[] { system })
                .Select(x => new DecimalDelta
                {
                    DeltaType = DeltaType.SystemDevelopment,
                    Id = x.Id,
                    Value = income * ((decimal)x.Development / (decimal)totalDevelopment)
                });
        }
    }
}