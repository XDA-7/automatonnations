using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_MilitaryCalculator
{
    public class ProductionForEmpire
    {
        private Mock<IConfiguration> _configuration = new Mock<IConfiguration>();
        private Mock<IRandom> _random = new Mock<IRandom>();
        private IMilitaryCalculator _militaryCalculator;

        private StarSystem[] _systems;
        private EmpireSystemsView _empire;

        public ProductionForEmpire()
        {
            _systems = new StarSystem[]
            {
                new StarSystem { Id = ObjectId.GenerateNewId(), Development = 300.0 },
                new StarSystem { Id = ObjectId.GenerateNewId(), Development = 450.0 },
                new StarSystem { Id = ObjectId.GenerateNewId(), Development = 250.0 },
                new StarSystem { Id = ObjectId.GenerateNewId(), Development = 800.0 },
                new StarSystem { Id = ObjectId.GenerateNewId(), Development = 200.0 }
            };

            _empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 1.0 }, Leaders = new Leader[0] },
                StarSystems = _systems
            };

            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
        }

        [Fact]
        public void ProductionIsZeroWhenNoSystemsPresent()
        {
            var empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 1.0 }, Leaders = new Leader[0] },
                StarSystems = new StarSystem[0]
            };

            var result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(0, result.EmpireProduction);
        }

        [Fact]
        public void ProductionIsZeroWhenTotalSystemDevelopmentIsZero()
        {
            foreach (var system in _systems)
            {
                system.Development = 0;
            }

            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(0, result.EmpireProduction);
        }

        [Fact]
        public void ProductionIsZeroWhenPowerAlignemntIsZero()
        {
            _empire.Empire.Alignment.Power = 0.0;
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(0, result.EmpireProduction);
        }

        [Fact]
        public void ProductionIsProportionalToTotalSystemDevelopment()
        {
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(2000, result.EmpireProduction);

            _systems[2].Development = 0.0;
            result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(1750.0, result.EmpireProduction);
        }

        [Fact]
        public void ProductionIsProportionalToPowerAlignment()
        {
            _empire.Empire.Alignment.Power = 0.8;
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(1600, result.EmpireProduction);

            _empire.Empire.Alignment.Power = 0.5;
            result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(1000, result.EmpireProduction);
        }

        [Fact]
        public void ProductionIsCappedWhenConfiguredTo()
        {
            _configuration.Setup(x => x.CapMilitaryProduction)
                .Returns(true);
            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
            var militaryCap = 2000.0 * Parameters.MilitaryCapDevelopmentProportion;

            _empire.Empire.Military = militaryCap;
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(0.0, result.EmpireProduction);

            _empire.Empire.Military = militaryCap - 500.0;
            result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(500.0, result.EmpireProduction);
        }

        [Fact]
        public void ProductionIsNegativeIfDevelopmentCannotSustainCurrentMilitary()
        {
            _configuration.Setup(x => x.CapMilitaryProduction)
                .Returns(true);
            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
            var militaryCap = 2000.0 * Parameters.MilitaryCapDevelopmentProportion;

            _empire.Empire.Military = militaryCap + 500.0;
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(-500.0, result.EmpireProduction);
        }

        [Fact]
        public void ReturnsLeadersWithWithheldMilitaryProductionAdded()
        {
            SetupLeaders();
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Contains(
                result.UpdatedLeaders,
                leader =>
                leader.StarSystemIds.Contains(_systems[0].Id) &&
                leader.StarSystemIds.Contains(_systems[1].Id) &&
                leader.Military == 675.0);
            Assert.Contains(
                result.UpdatedLeaders,
                leader =>
                leader.StarSystemIds.Contains(_systems[3].Id) &&
                leader.StarSystemIds.Contains(_systems[4].Id) &&
                leader.Military == 300.0);
        }

        [Fact]
        public void SubtractsMilitaryWithheldByLeadersFromEmpireProduction()
        {
            SetupLeaders();
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(1525.0, result.EmpireProduction);
        }

        [Fact]
        public void LeaderMilitaryIsCappedWhenConfiguredTo()
        {
            SetupLeaders();
            var militaryCap = 1000.0 * Parameters.MilitaryCapDevelopmentProportion;
            _empire.Empire.Leaders.ToArray()[1].Military = militaryCap;
            _configuration
                .Setup(x => x.CapMilitaryProduction)
                .Returns(true);
            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Contains(
                result.UpdatedLeaders,
                leader =>
                leader.StarSystemIds.Contains(_systems[3].Id) &&
                leader.StarSystemIds.Contains(_systems[4].Id) &&
                leader.Military == militaryCap);
        }

        private void SetupLeaders()
        {
            _empire.Empire.Leaders = new Leader[]
            {
                new Leader
                {
                    StarSystemIds = new ObjectId[] { _systems[0].Id, _systems[1].Id },
                    MilitaryWitholdingRate = 0.5,
                    Military = 300.0
                },
                new Leader {
                    StarSystemIds = new ObjectId[] { _systems[3].Id, _systems[4].Id },
                    MilitaryWitholdingRate = 0.1,
                    Military = 200.0
                }
            };
        }
    }
}