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

        private SimulationView _simulationView = new SimulationView
        {
            Simulation = new Simulation(),
            StarSystems = new StarSystem[0],
            Empires = new Empire[0],
            Wars = new War[0]
        };
        private DeltaSet _deltaSet = new DeltaSet
        {
            Deltas = new Delta[0],
            DeltaDoubles = new Delta<double>[0],
            DeltaObjectIds = new Delta<ObjectId>[0]
        };

        public GetForTick()
        {
            _deltaApplier = new DeltaApplier(_deltaRepository.Object, _simulationRepository.Object);
            _simulationRepository
                .Setup(x => x.GetSimulationView(It.IsAny<ObjectId>()))
                .Returns(_simulationView);
            _deltaRepository
                .Setup(x => x.GetForSimulation(It.IsAny<ObjectId>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_deltaSet);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(20, 0)]
        [InlineData(20, 20)]
        [InlineData(345, 221)]
        public void GetsDeltasAfterTick(int simulationAge, int tick)
        {
            _simulationView.Simulation = new Simulation { Ticks = simulationAge };
            _deltaApplier.GetForTick(new ObjectId(), tick);
            _deltaRepository
                .Verify(x => x.GetForSimulation(It.IsAny<ObjectId>(), tick, simulationAge), Times.Once);
        }

        [Fact]
        public void AppliesSystemDevelopment()
        {
            var starSystem = new StarSystem { Id = ObjectId.GenerateNewId(), Development = 430.0 };
            _simulationView.StarSystems = new StarSystem[] { starSystem };
            _deltaSet.DeltaDoubles =  new Delta<double>[]
            {
                new Delta<double> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = starSystem.Id, Value = 120.0 },
                new Delta<double> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = ObjectId.GenerateNewId(), Value = 90.0 },
                new Delta<double> { DeltaType = DeltaType.SystemDevelopment, ReferenceId = starSystem.Id, Value = 35.0 }
            };
            
            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            Assert.Equal(275.0, result.StarSystems.Single().Development);
        }

        [Fact]
        public void AppliesEmpireSystemGain()
        {
            var starSystem = new StarSystem { Id = ObjectId.GenerateNewId() };
            var empire = new Empire { Id = ObjectId.GenerateNewId() , StarSystemsIds = new ObjectId[] { starSystem.Id } };
            _simulationView.Empires = new Empire[] { empire };
            _simulationView.StarSystems = new StarSystem[] { starSystem };
            _deltaSet.DeltaObjectIds = new Delta<ObjectId>[]
            {
                new Delta<ObjectId> { DeltaType = DeltaType.EmpireSystemGain, ReferenceId = empire.Id, Value = starSystem.Id }
            };

            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            var resultEmpire = Assert.Single(result.Empires);
            Assert.Empty(resultEmpire.StarSystemsIds);
        }

        [Fact]
        public void AppliesEmpireSystemLoss()
        {
            var starSystem = new StarSystem { Id = ObjectId.GenerateNewId() };
            var empire = new Empire { Id = ObjectId.GenerateNewId(), StarSystemsIds = new ObjectId[0] };
            _simulationView.Empires = new Empire[] { empire };
            _simulationView.StarSystems = new StarSystem[] { starSystem };
            _deltaSet.DeltaObjectIds = new Delta<ObjectId>[]
            {
                new Delta<ObjectId> { DeltaType = DeltaType.EmpireSystemLoss, ReferenceId = empire.Id, Value = starSystem.Id }
            };

            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            var resultEmpire = Assert.Single(result.Empires);
            var resultSystemId = Assert.Single(resultEmpire.StarSystemsIds);
            Assert.Equal(starSystem.Id, resultSystemId);
        }

        [Fact]
        public void AppliesWarBegin()
        {
            var war = new War { Id = ObjectId.GenerateNewId() };
            _simulationView.Wars = new War[] { war };
            _deltaSet.Deltas = new Delta[]
            {
                new Delta { DeltaType = DeltaType.WarBegin, ReferenceId = war.Id }
            };

            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            Assert.Empty(result.Wars);
        }

        [Fact]
        public void AppliesWarEnd()
        {
            var war = new War { Id = ObjectId.GenerateNewId(), Active = false };
            _simulationView.Wars = new War[] { war };
            _deltaSet.Deltas = new Delta[]
            {
                new Delta { DeltaType = DeltaType.WarEnd, ReferenceId = war.Id }
            };

            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            var resultWar = Assert.Single(result.Wars);
            Assert.True(resultWar.Active);
        }

        [Fact]
        public void AppliesWarAttackerDamage()
        {
            var war = new War { Id = ObjectId.GenerateNewId(), AttackerDamage = 500.0 };
            _simulationView.Wars = new War[] { war };
            _deltaSet.DeltaDoubles = new Delta<double>[]
            {
                new Delta<double> { DeltaType = DeltaType.WarAttackerDamage, ReferenceId = war.Id, Value = 216.5 }
            };

            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            var resultWar = Assert.Single(result.Wars);
            Assert.Equal(283.5, war.AttackerDamage);
        }

        [Fact]
        public void AppliesWarDefenderDamage()
        {
            var war = new War { Id = ObjectId.GenerateNewId(), DefenderDamage = 500.0 };
            _simulationView.Wars = new War[] { war };
            _deltaSet.DeltaDoubles = new Delta<double>[]
            {
                new Delta<double> { DeltaType = DeltaType.WarDefenderDamage, ReferenceId = war.Id, Value = 216.5 }
            };

            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            var resultWar = Assert.Single(result.Wars);
            Assert.Equal(283.5, war.DefenderDamage);
        }

        [Fact]
        public void AppliesEmpireMilitary()
        {
            var empire = new Empire { Id = ObjectId.GenerateNewId(), StarSystemsIds = new ObjectId[0], Military = 230.0 };
            _simulationView.Empires = new Empire[] { empire };
            _deltaSet.DeltaDoubles = new Delta<double>[]
            {
                new Delta<double> { DeltaType = DeltaType.EmpireMilitary, ReferenceId = empire.Id, Value = 45.0 }
            };

            var result = _deltaApplier.GetForTick(It.IsAny<ObjectId>(), It.IsAny<int>());
            var resultEmpire = Assert.Single(result.Empires);
            Assert.Equal(185.0, empire.Military);
        }
    }
}