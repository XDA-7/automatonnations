using System;
using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class DevelopmentCalculator : IDevelopmentCalculator
    {
        private ConnectedSystemsOnlyDelegate _connectedSystemsOnlyDelegate;

        public DevelopmentCalculator(IConfiguration configuration)
        {
            if (configuration.DevelopmentCalculation == DevelopmentCalculation.ProportionalDistribution)
            {
                _connectedSystemsOnlyDelegate = ProportionalDistribution;
            }
            else if (configuration.DevelopmentCalculation == DevelopmentCalculation.EqualDistribution)
            {
                _connectedSystemsOnlyDelegate = EqualDistribution;
            }
            else if (configuration.DevelopmentCalculation == DevelopmentCalculation.SelfPriorityThenEqual)
            {
                _connectedSystemsOnlyDelegate = SelfPriorityThenEqual;
            }
        }

        public void SetConnectedSystemsOnlyHook(ConnectedSystemsOnlyDelegate del) => _connectedSystemsOnlyDelegate = del;

        public IEnumerable<GrowthFromSystemResult> GrowthFromSystem(StarSystem starSystem, EmpireSystemsView empireView)
        {
            var connectedSystems = empireView.StarSystems.Where(x => starSystem.ConnectedSystemIds.Contains(x.Id));
            var income = starSystem.Development * Parameters.IncomeRate * empireView.Empire.Alignment.Prosperity;
            if (income == 0)
            {
                return new GrowthFromSystemResult[0];
            }

            return _connectedSystemsOnlyDelegate(starSystem, connectedSystems, income);
        }


        private IEnumerable<GrowthFromSystemResult> ProportionalDistribution(StarSystem system, IEnumerable<StarSystem> connectedSystems, double income)
        {
            var totalDevelopment = system.Development + connectedSystems.Sum(x => x.Development);
            return connectedSystems
                .Concat(new StarSystem[] { system })
                .Select(x => new GrowthFromSystemResult(x.Id, income * (x.Development / totalDevelopment)));
        }

        private IEnumerable<GrowthFromSystemResult> EqualDistribution(StarSystem system, IEnumerable<StarSystem> connectedSystems, double income)
        {
            var systemCount = connectedSystems.Count() + 1;
            return connectedSystems
                .Concat(new StarSystem[] { system })
                .Select(x => new GrowthFromSystemResult(x.Id, income / systemCount));
        }

        private IEnumerable<GrowthFromSystemResult> SelfPriorityThenEqual(StarSystem system, IEnumerable<StarSystem> connectedSystems, double income)
        {
            var selfIncome = income * Parameters.IncomeReservedForSelf;
            var neighboursIncome = income - selfIncome;
            var neighbourCount = (double)connectedSystems.Count();
            var selfGrowth = new GrowthFromSystemResult(system.Id, selfIncome);
            var neighboursGrowth = connectedSystems.Select(x => new GrowthFromSystemResult(x.Id, neighboursIncome / neighbourCount));
            return neighboursGrowth.Concat(new GrowthFromSystemResult[] { selfGrowth });
        }
    }

    public delegate IEnumerable<GrowthFromSystemResult> ConnectedSystemsOnlyDelegate(StarSystem system, IEnumerable<StarSystem> connectedSystems, double income);
}