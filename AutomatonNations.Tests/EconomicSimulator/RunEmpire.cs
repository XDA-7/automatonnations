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
            var view = SetupGrowthCalculator();
            var starSystems = view.StarSystems.ToArray();

            _economicSimulator.RunEmpire(new DeltaMetadata(ObjectId.Empty, 0), view);

            _starSystemRepository
                .Verify(
                    x => x.ApplyDevelopment(
                        It.Is<IEnumerable<Delta<double>>>(
                            y =>
                            ContainsSystemAndValue(y, starSystems[2], 300.0) &&
                            ContainsSystemAndValue(y, starSystems[4], 150.0) &&
                            ContainsSystemAndValue(y, starSystems[2], 20.0) &&
                            ContainsSystemAndValue(y, starSystems[4], 450.0) &&
                            ContainsSystemAndValue(y, starSystems[3], 270.0) &&
                            ContainsSystemAndValue(y, starSystems[2], 80.0) &&
                            ContainsSystemAndValue(y, starSystems[0], 90.0) &&
                            ContainsSystemAndValue(y, starSystems[1], 230.0)
                        )
                    ),
                    Times.Once
                );
        }

        [Fact]
        public void AppliesMetadataToDeltas()
        {
            var view = SetupGrowthCalculator();
            var metadata = new DeltaMetadata(ObjectId.GenerateNewId(), 51);

            _economicSimulator.RunEmpire(metadata, view);

            _starSystemRepository.Verify(x => x.ApplyDevelopment(
                It.Is<IEnumerable<Delta<double>>>(
                    deltas => deltas.All(
                        delta =>
                            delta.SimulationId == metadata.SimulationId &&
                            delta.Tick == metadata.Tick))),
                Times.Once);
        }

        [Fact]
        public void UpdatesSystemDevelopmentInView()
        {
            var view = SetupGrowthCalculator();
            var starSystems = view.StarSystems.ToArray();

            _economicSimulator.RunEmpire(new DeltaMetadata(ObjectId.Empty, 0), view);

            Assert.Equal(90.0, starSystems[0].Development);
            Assert.Equal(230.0, starSystems[1].Development);
            Assert.Equal(400.0, starSystems[2].Development);
            Assert.Equal(270.0, starSystems[3].Development);
            Assert.Equal(600.0, starSystems[4].Development);
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
            
            _economicSimulator.RunEmpire(new DeltaMetadata(ObjectId.Empty, 0), view);

            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    starSystems[0],
                    It.Is<IEnumerable<StarSystem>>(y => y.Count() == 0),
                    It.IsAny<double>()
                ),
                Times.Once);
            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    starSystems[1],
                    It.Is<IEnumerable<StarSystem>>(y => y.Count() == 2 && y.Contains(starSystems[0]) && y.Contains(starSystems[2])),
                    It.IsAny<double>()
                ),
                Times.Once);
            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    starSystems[2],
                    It.Is<IEnumerable<StarSystem>>(y => y.Count() == 0),
                    It.IsAny<double>()
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
                    Alignment = new Alignment { Prosperity = 0.21 }
                },
                StarSystems = new StarSystem[] { starSystem }
            };

            _economicSimulator.RunEmpire(new DeltaMetadata(ObjectId.Empty, 0), view);

            _developmentCalculator
                .Verify(x => x.GrowthFromSystem(
                    It.IsAny<StarSystem>(),
                    It.IsAny<IEnumerable<StarSystem>>(),
                    0.21
                ),
                Times.Once);
        }

        private bool ContainsSystemAndValue(IEnumerable<Delta<double>> deltas, StarSystem starSystem, double value) =>
            deltas.Any(delta => delta.ReferenceId == starSystem.Id && delta.Value == value);

        private EmpireSystemsView SetupGrowthCalculator()
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
                .Setup(x => x.GrowthFromSystem(starSystems[0], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<double>()))
                .Returns(new GrowthFromSystemResult[]
                {
                    new GrowthFromSystemResult(starSystems[2].Id, 300.0),
                    new GrowthFromSystemResult(starSystems[4].Id, 150.0)
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[1], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<double>()))
                .Returns(new GrowthFromSystemResult[]
                {
                    new GrowthFromSystemResult(starSystems[2].Id, 20.0),
                    new GrowthFromSystemResult(starSystems[4].Id, 450.0)
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[2], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<double>()))
                .Returns(new GrowthFromSystemResult[]
                {
                    new GrowthFromSystemResult(starSystems[3].Id, 270.0)
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[3], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<double>()))
                .Returns(new GrowthFromSystemResult[]
                {
                    new GrowthFromSystemResult(starSystems[2].Id, 80.0)
                });
            _developmentCalculator
                .Setup(x => x.GrowthFromSystem(starSystems[4], It.IsAny<IEnumerable<StarSystem>>(), It.IsAny<double>()))
                .Returns(new GrowthFromSystemResult[]
                {
                    new GrowthFromSystemResult(starSystems[0].Id, 90.0),
                    new GrowthFromSystemResult(starSystems[1].Id, 230.0)
                });
            
            return new EmpireSystemsView
            {
                Empire = new Empire()
                {
                    StarSystemsIds = starSystems.Select(x => x.Id),
                    Alignment = new Alignment()
                },
                StarSystems = starSystems
            };
        }
    }
}