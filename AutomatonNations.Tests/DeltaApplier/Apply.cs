using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations
{
    public class Apply
    {
        private Mock<IDeltaRepository> _deltaRepository = new Mock<IDeltaRepository>();
        private Mock<ISimulationRepository> _simulationRepository = new Mock<ISimulationRepository>();
        private IDeltaApplier _deltaApplier;

        public Apply()
        {
            _deltaApplier = new DeltaApplier(_deltaRepository.Object, _simulationRepository.Object);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(20, 0)]
        [InlineData(20, 20)]
        [InlineData(345, 221)]
        public void GetsDeltasWithinBackTickRange(int simulationAge, int backTicks)
        {
            _simulationRepository
                .Setup(x => x.GetSimulationView(It.IsAny<ObjectId>()))
                .Returns(new SimulationView
                {
                    Ticks = simulationAge,
                    StarSystems = new StarSystem[0]
                });
            _deltaRepository
                .Setup(x => x.GetForSimulation(It.IsAny<ObjectId>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DeltaSet
                {
                    DeltaDecimals = new Delta<decimal>[0]
                });
            _deltaApplier.Apply(new ObjectId(), backTicks);
            _deltaRepository
                .Verify(x => x.GetForSimulation(It.IsAny<ObjectId>(), simulationAge - backTicks, simulationAge), Times.Once);
        }

        [Fact]
        public void AppliesSystemDevelopmentDeltas()
        {
            var starSystem = new StarSystem { Id = ObjectId.GenerateNewId(), Development = 430M };
            _simulationRepository
                .Setup(x => x.GetSimulationView(It.IsAny<ObjectId>()))
                .Returns(new SimulationView { StarSystems = new StarSystem[] { starSystem } });
            _deltaRepository
                .Setup(x => x.GetForSimulation(It.IsAny<ObjectId>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DeltaSet
                {
                    DeltaDecimals = new Delta<decimal>[]
                    {
                        new Delta<decimal> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = starSystem.Id, Value = 120M },
                        new Delta<decimal> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = ObjectId.GenerateNewId(), Value = 90M },
                        new Delta<decimal> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = starSystem.Id, Value = 35M }
                    }
                });
            
            var result = _deltaApplier.Apply(It.IsAny<ObjectId>(), It.IsAny<int>());

            Assert.Equal(275M, result.StarSystems.Single().Development);
        }
    }
}