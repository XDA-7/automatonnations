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
                new StarSystem { Development = 300.0 },
                new StarSystem { Development = 450.0 },
                new StarSystem { Development = 250.0 },
                new StarSystem { Development = 800.0 },
                new StarSystem { Development = 200.0 }
            };

            _empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 1.0 } },
                StarSystems = _systems
            };

            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
        }

        [Fact]
        public void ProductionIsZeroWhenNoSystemsPresent()
        {
            var empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 1.0 } },
                StarSystems = new StarSystem[0]
            };

            var result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(0, result);
        }

        [Fact]
        public void ProductionIsZeroWhenTotalSystemDevelopmentIsZero()
        {
            foreach (var system in _systems)
            {
                system.Development = 0;
            }

            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(0, result);
        }

        [Fact]
        public void ProductionIsZeroWhenPowerAlignemntIsZero()
        {
            _empire.Empire.Alignment.Power = 0.0;
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(0, result);
        }

        [Fact]
        public void ProductionIsProportionalToTotalSystemDevelopment()
        {
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(2000, result);

            _systems[2].Development = 0.0;
            result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(1750.0, result);
        }

        [Fact]
        public void ProductionIsProportionalToPowerAlignment()
        {
            _empire.Empire.Alignment.Power = 0.8;
            var result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(1600, result);

            _empire.Empire.Alignment.Power = 0.5;
            result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(1000, result);
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
            Assert.Equal(0.0, result);

            _empire.Empire.Military = militaryCap - 500.0;
            result = _militaryCalculator.ProductionForEmpire(_empire);
            Assert.Equal(500.0, result);
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
            Assert.Equal(-500.0, result);
        }
    }
}