using Moq;
using Xunit;

namespace AutomatonNations.Tests_MilitaryCalculator
{
    public class ProductionForEmpire
    {
        private Mock<IRandom> _random = new Mock<IRandom>();
        private IMilitaryCalculator _militaryCalculator;

        public ProductionForEmpire()
        {
            _militaryCalculator = new MilitaryCalculator(_random.Object);
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
            var empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 1.0 } },
                StarSystems = new StarSystem[]
                {
                    new StarSystem(),
                    new StarSystem(),
                    new StarSystem(),
                    new StarSystem(),
                    new StarSystem()
                }
            };

            var result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(0, result);
        }

        [Fact]
        public void ProductionIsZeroWhenPowerAlignemntIsZero()
        {
            var empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 0.0 } },
                StarSystems = new StarSystem[]
                {
                    new StarSystem { Development = 300.0 },
                    new StarSystem { Development = 450.0 },
                    new StarSystem { Development = 243.0 },
                    new StarSystem { Development = 780.3 },
                    new StarSystem { Development = 213.5 }
                }
            };

            var result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(0, result);
        }

        [Fact]
        public void ProductionIsProportionalToTotalSystemDevelopment()
        {
            var systems = new StarSystem[]
            {
                    new StarSystem { Development = 300.0 },
                    new StarSystem { Development = 450.0 },
                    new StarSystem { Development = 250.0 },
                    new StarSystem { Development = 800.0 },
                    new StarSystem { Development = 200.0 }
            };
            var empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 1.0 } },
                StarSystems = systems
            };

            var result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(2000, result);

            systems[2].Development = 0.0;

            result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(1750.0, result);
        }

        [Fact]
        public void ProductionIsProportionalToPowerAlignment()
        {
            var systems = new StarSystem[]
            {
                    new StarSystem { Development = 300.0 },
                    new StarSystem { Development = 450.0 },
                    new StarSystem { Development = 250.0 },
                    new StarSystem { Development = 800.0 },
                    new StarSystem { Development = 200.0 }
            };
            var empire = new EmpireSystemsView
            {
                Empire = new Empire { Alignment = new Alignment { Power = 0.8 } },
                StarSystems = systems
            };

            var result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(1600, result);

            empire.Empire.Alignment.Power = 0.5;

            result = _militaryCalculator.ProductionForEmpire(empire);

            Assert.Equal(1000, result);
        }
    }
}