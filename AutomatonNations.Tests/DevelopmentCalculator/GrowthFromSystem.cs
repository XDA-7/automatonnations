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
            var result = _developmentCalculator.GrowthFromSystem(new StarSystem(), new StarSystem[0], 1.0M);
            Assert.Empty(result);
        }

        [Fact]
        public void ReturnsDeltasOfTypeSystemDevelopment()
        {
            var result = _developmentCalculator.GrowthFromSystem(new StarSystem { Development = 1 }, new StarSystem[0], 1.0M);
            Assert.Single(result, delta => delta.DeltaType == DeltaType.SystemDevelopment);
        }

        [Fact]
        public void DistributesGrowthBetweenAllSystemsProportionalToDevelopmenmt()
        {
            var system = new StarSystem { Development = 100, Id = ObjectId.GenerateNewId() };
            var connectedSystems = new StarSystem[]
            {
                new StarSystem { Development = 300, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 200, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 400, Id = ObjectId.GenerateNewId() }
            };

            var result = _developmentCalculator.GrowthFromSystem(system, connectedSystems, 1.0M);

            var income = system.Development * Parameters.IncomeRate;
            Assert.Contains(result, delta => delta.ReferenceId == system.Id && delta.Value == 0.1M * income);
            Assert.Contains(result, delta => delta.ReferenceId == connectedSystems[0].Id && delta.Value == 0.3M * income);
            Assert.Contains(result, delta => delta.ReferenceId == connectedSystems[1].Id && delta.Value == 0.2M * income);
            Assert.Contains(result, delta => delta.ReferenceId == connectedSystems[2].Id && delta.Value == 0.4M * income);
        }

        [Fact]
        public void AppliesGrowthAsFunctionOfIncomeAndGrowthFocus()
        {
            var system = new StarSystem { Development = 100, Id = ObjectId.GenerateNewId() };
            var connectedSystems = new StarSystem[0];

            var result = _developmentCalculator.GrowthFromSystem(system, connectedSystems, 0.30M);

            var growth = system.Development * Parameters.IncomeRate * 0.30M;
            Assert.Contains(result, delta => delta.ReferenceId == system.Id && delta.Value == growth);
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

            var result = _developmentCalculator.GrowthFromSystem(system, connectedSystems, 1.0M);

            Assert.Equal(100, system.Development);
            Assert.Equal(300, connectedSystems[0].Development);
            Assert.Equal(200, connectedSystems[1].Development);
            Assert.Equal(400, connectedSystems[2].Development);
        }
    }
}