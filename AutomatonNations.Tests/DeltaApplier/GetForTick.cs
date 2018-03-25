using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations
{
    public class GetForTick
    {
        private Mock<IDeltaRepository> _deltaRepository = new Mock<IDeltaRepository>();
        private Mock<ISimulationRepository> _simulationRepository = new Mock<ISimulationRepository>();
        private IDeltaApplier _deltaApplier;

        public GetForTick()
        {
            _deltaApplier = new DeltaApplier(_deltaRepository.Object, _simulationRepository.Object);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(20, 0)]
        [InlineData(20, 20)]
        [InlineData(345, 221)]
        public void GetsDeltasAfterTick(int simulationAge, int tick)
        {
            _simulationRepository
                .Setup(x => x.GetSimulationView(It.IsAny<ObjectId>()))
                .Returns(new SimulationView
                {
                    Simulation = new Simulation { Ticks = simulationAge },
                    StarSystems = new StarSystem[0]
                });
            _deltaRepository
                .Setup(x => x.GetForSimulation(It.IsAny<ObjectId>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DeltaSet
                {
                    DeltaDoubles = new Delta<double>[0]
                });
            _deltaApplier.GetForTick(new ObjectId(), tick);
            _deltaRepository
                .Verify(x => x.GetForSimulation(It.IsAny<ObjectId>(), tick, simulationAge), Times.Once);
        }

        [Fact]
        public void AppliesSystemDevelopmentDeltas()
        {
            var starSystem = new StarSystem { Id = ObjectId.GenerateNewId(), Development = 430.0 };
            _simulationRepository
                .Setup(x => x.GetSimulationView(It.IsAny<ObjectId>()))
                .Returns(new SimulationView
                {
                    Simulation = new Simulation(),
                    StarSystems = new StarSystem[] { starSystem }
                });
            _deltaRepository
                .Setup(x => x.GetForSimulation(It.IsAny<ObjectId>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DeltaSet
                {
                    DeltaDoubles = new Delta<double>[]
                    {
                        new Delta<double> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = starSystem.Id, Value = 120.0 },
                        new Delta<double> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = ObjectId.GenerateNewId(), Value = 90.0 },
                        new Delta<double> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = starSystem.Id, Value = 35.0 }
                    }
                });
            
            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());

            Assert.Equal(275.0, result.StarSystems.Single().Development);
        }
    }
}