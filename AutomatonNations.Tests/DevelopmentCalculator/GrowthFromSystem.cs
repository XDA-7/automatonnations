using System.Collections.Generic;
using MongoDB.Bson;
using Xunit;

namespace AutomatonNations.Tests_DevelopmentCalculator
{
    public class GrowthFromSystem
    {
        private IDevelopmentCalculator _developmentCalculator;

        public GrowthFromSystem()
        {
            _developmentCalculator = new DevelopmentCalculator();
        }

        [Fact]
        public void ReturnsEmptyCollectionIfSystemHasZeroDevelopment()
        {
            var result = _developmentCalculator.GrowthFromSystem(new StarSystem(), new StarSystem[0], 1.0);
            Assert.Empty(result);
        }

        [Fact]
        public void ReturnsEmptyCollectionIfEmpireHasZeroProperityFocus()
        {
            var result = _developmentCalculator.GrowthFromSystem(new StarSystem { Development = 10000.0 }, new StarSystem[0], 0.0);
            Assert.Empty(result);
        }

        [Fact]
        public void DistributesGrowthBetweenAllSystemsProportionalToDevelopmenmt()
        {
            var system = new StarSystem { Development = 100.0, Id = ObjectId.GenerateNewId() };
            var connectedSystems = new StarSystem[]
            {
                new StarSystem { Development = 300, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 200, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 400, Id = ObjectId.GenerateNewId() }
            };

            var result = _developmentCalculator.GrowthFromSystem(system, connectedSystems, 1.0);

            var income = system.Development * Parameters.IncomeRate;
            Assert.Contains(result, value => value.SystemId == system.Id && value.Growth == 0.1 * income);
            Assert.Contains(result, value => value.SystemId == connectedSystems[0].Id && value.Growth == 0.3 * income);
            Assert.Contains(result, value => value.SystemId == connectedSystems[1].Id && value.Growth == 0.2 * income);
            Assert.Contains(result, value => value.SystemId == connectedSystems[2].Id && value.Growth == 0.4 * income);
        }

        [Fact]
        public void AppliesGrowthAsFunctionOfIncomeAndGrowthFocus()
        {
            var system = new StarSystem { Development = 100, Id = ObjectId.GenerateNewId() };
            var connectedSystems = new StarSystem[0];

            var result = _developmentCalculator.GrowthFromSystem(system, connectedSystems, 0.30);

            var growth = system.Development * Parameters.IncomeRate * 0.30;
            Assert.Contains(result, value => value.SystemId == system.Id && value.Growth == growth);
        }

        [Fact]
        public void DoesNotModifyDevelopmentOfSystemsDirectly()
        {
            var system = new StarSystem { Development = 100, Id = ObjectId.GenerateNewId() };
            var connectedSystems = new StarSystem[]
            {
                new StarSystem { Development = 300, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 200, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 400, Id = ObjectId.GenerateNewId() }
            };

            var result = _developmentCalculator.GrowthFromSystem(system, connectedSystems, 1.0);

            Assert.Equal(100, system.Development);
            Assert.Equal(300, connectedSystems[0].Development);
            Assert.Equal(200, connectedSystems[1].Development);
            Assert.Equal(400, connectedSystems[2].Development);
        }
    }
}