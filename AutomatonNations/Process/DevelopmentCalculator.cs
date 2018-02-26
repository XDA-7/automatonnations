using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class DevelopmentCalculator : IDevelopmentCalculator
    {
        public IEnumerable<Delta<decimal>> GrowthFromSystem(StarSystem system, IEnumerable<StarSystem> connectedSystems, decimal growthFocus)
        {
            var income = system.Development * Parameters.IncomeRate * growthFocus;
            if (income == 0)
            {
                return new Delta<decimal>[0];
            }

            var totalDevelopment = system.Development + connectedSystems.Sum(x => x.Development);
            return connectedSystems
                .Concat(new StarSystem[] { system })
                .Select(x => new Delta<decimal>
                {
                    DeltaType = DeltaType.SystemDevelopment,
                    ReferenceId = x.Id,
                    Value = income * ((decimal)x.Development / (decimal)totalDevelopment)
                });
        }
    }
}