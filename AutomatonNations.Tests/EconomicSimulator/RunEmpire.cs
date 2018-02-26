using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations
{
    public class RunEmpire
    {
        private Mock<IStarSystemRepository> _starSystemRepository = new Mock<IStarSystemRepository>();
        private Mock<IDevelopmentCalculator> _developmentCalculator = new Mock<IDevelopmentCalculator>();
        private IEconomicSimulator _economicSimulator;

        public RunEmpire()
        {
            _economicSimulator = new EconomicSimulator(_starSystemRepository.Object, _developmentCalculator.Object);
        }

        [Fact]
        public void UpdatesSystemDevelopmentInRepository()
        {
            var starSystems = SetupGrowthCalculator();
            var empire = new Empire()
            {
                StarSystemsIds = starSystems.Select(x => x.Id),
                Alignment = new Alignment()
            };
            var view = new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = starSystems
            };

            _economicSimulator.RunEmpire(view);

            _starSystemRepository
                .Verify(
                    x => x.ApplyDevelopment(
                        It.Is<IEnumerable<Delta<decimal>>>(
                            y =>
                            ContainsSystemAndValue(y, starSystems[2], 300M) &&
                            ContainsSystemAndValue(y, starSystems[4], 150M) &&
                            ContainsSystemAndValue(y, starSystems[2], 20M) &&
                            ContainsSystemAndValue(y, starSystems[4], 450M) &&
                            ContainsSystemAndValue(y, starSystems[3], 270M) &&
                            ContainsSystemAndValue(y, starSystems[2], 80M) &&
                            ContainsSystemAndValue(y, starSystems[0], 90M) &&
                            ContainsSystemAndValue(y, starSystems[1], 230M)
                        )
                    ),
                    Times.Once
                );
        }

        [Fact]
        public void UpdatesSystemDevelopmentInView()
        {
            var starSystems = SetupGrowthCalculator();
            var empire = new Empire()
            {
                StarSystemsIds = starSystems.Select(x => x.Id),
                Alignment = new Alignment()
            };
            var view = new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = starSystems
            };

            _economicSimulator.RunEmpire(view);

            Assert.Equal(90M, starSystems[0].Development);
            Assert.Equal(230M, starSystems[1].Development);
            Assert.Equal(400M, starSystems[2].Development);
            Assert.Equal(270M, starSystems[3].Development);
            Assert.Equal(600M, starSystems[4].Development);
        }

        [Fact]
        public void CalculatesGrowthUsingConnectedSystemsWithinEmpire()
        {
            var starSystems = new StarSystem[]
            {
                new StarSystem() { Id = ObjectId.GenerateNewId() },
                new StarSystem() { Id = ObjectId.GenerateNewId() },
                new StarSystem() { Id = ObjectId.GenerateNewId() }
            };
            var empire = new Empire()
            {
                StarSystemsIds = starSystems.Select(x => x.Id),
                Alignment = new Alignment()
            };
            var view = new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = starSystems
            };

            starSystems[0].ConnectedSystemIds = new ObjectId[]
            {
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId()
            };
            starSystems[1].ConnectedSystemIds = new ObjectId[]
            {
                starSystems[0].Id,
                starSystems[2].Id,
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId()
            };
            starSystems[2].ConnectedSystemIds = new ObjectId[0];
            
            _economicSimulator.RunEmpire(view);

            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    starSystems[0],
                    It.Is<IEnumerable<StarSystem>>(y => y.Count() == 0),
                    It.IsAny<decimal>()
                ),
                Times.Once);
            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    starSystems[1],
                    It.Is<IEnumerable<StarSystem>>(y => y.Count() == 2 && y.Contains(starSystems[0]) && y.Contains(starSystems[2])),
                    It.IsAny<decimal>()
                ),
                Times.Once);
            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    starSystems[2],
                    It.Is<IEnumerable<StarSystem>>(y => y.Count() == 0),
                    It.IsAny<decimal>()
                ),
                Times.Once);
        }

        [Fact]
        public void CalculatesGrowthUsingGrowthFocusFromAlignment()
        {
            var starSystem = new StarSystem();
            var view = new EmpireSystemsView
            {
                Empire = new Empire
                {
                    StarSystemsIds = new ObjectId[] { starSystem.Id },
                    Alignment = new Alignment { Prosperity = 0.21M }
                },
                StarSystems = new StarSystem[] { starSystem }
            };

            _economicSimulator.RunEmpire(view);

            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    It.IsAny<StarSystem>(),
                    It.IsAny<IEnumerable<StarSystem>>(),
                    0.21M
                ),
                Times.Once);
        }

        private bool ContainsSystemAndValue(IEnumerable<Delta<decimal>> deltas, StarSystem starSystem, decimal value) =>
            deltas.Any(delta => delta.ReferenceId == starSystem.Id && delta.Value == value);

        private StarSystem[] SetupGrowthCalculator()
        {
            var starSystems = new StarSystem[]
            {
                new StarSystem
                {
                    Id = ObjectId.GenerateNewId(),
                },
                new StarSystem
                {
                    Id = ObjectId.GenerateNewId(),
                },
                new StarSystem
                {
                    Id = ObjectId.GenerateNewId(),
                },
                new StarSystem
                {
                    Id = ObjectId.GenerateNewId(),
                },
                new StarSystem
                {
                    Id = ObjectId.GenerateNewId(),
                }
            };

            starSystems[0].ConnectedSystemIds = new ObjectId[] { starSystems[2].Id, starSystems[4].Id };
            starSystems[1].ConnectedSystemIds = new ObjectId[] { starSystems[2].Id, starSystems[4].Id };
            starSystems[2].ConnectedSystemIds = new ObjectId[] { starSystems[3].Id };
            starSystems[3].ConnectedSystemIds = new ObjectId[] { starSystems[2].Id };
            starSystems[4].ConnectedSystemIds = new ObjectId[] { starSystems[0].Id, starSystems[1].Id };

            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[0], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<decimal>()))
                .Returns(new Delta<decimal>[]
                {
                    new Delta<decimal> { ReferenceId = starSystems[2].Id, Value = 300M },
                    new Delta<decimal> { ReferenceId = starSystems[4].Id, Value = 150M }
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[1], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<decimal>()))
                .Returns(new Delta<decimal>[]
                {
                    new Delta<decimal> { ReferenceId = starSystems[2].Id, Value = 20M },
                    new Delta<decimal> { ReferenceId = starSystems[4].Id, Value = 450M }
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[2], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<decimal>()))
                .Returns(new Delta<decimal>[]
                {
                    new Delta<decimal> { ReferenceId = starSystems[3].Id, Value = 270M }
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[3], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<decimal>()))
                .Returns(new Delta<decimal>[]
                {
                    new Delta<decimal> { ReferenceId = starSystems[2].Id, Value = 80M }
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[4], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<decimal>()))
                .Returns(new Delta<decimal>[]
                {
                    new Delta<decimal> { ReferenceId = starSystems[0].Id, Value = 90M },
                    new Delta<decimal> { ReferenceId = starSystems[1].Id, Value = 230M }
                });
            
            return starSystems;
        }
    }
}