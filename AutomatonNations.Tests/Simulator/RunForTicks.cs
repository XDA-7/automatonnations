using System.Collections.Generic;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_Simulator
{
    public class RunForTicks
    {
        private Mock<ISectorGenerator> _sectorGenerator = new Mock<ISectorGenerator>();
        private Mock<ISimulationRepository> _simulationRepository = new Mock<ISimulationRepository>();
        private Mock<IEmpireGenerator> _empireGenerator = new Mock<IEmpireGenerator>();
        private Mock<IEconomicSimulator> _economicSimulator = new Mock<IEconomicSimulator>();
        private Mock<IMilitarySimulator> _militarySimulator = new Mock<IMilitarySimulator>();
        private Mock<IDiplomacySimulator> _diplomacySimulator = new Mock<IDiplomacySimulator>();
        private Mock<ILeaderUpdater> _leaderUpdater = new Mock<ILeaderUpdater>();
        private Mock<IDeltaApplier> _deltaApplier = new Mock<IDeltaApplier>();
        private ISimulator _simulator;

        public RunForTicks()
        {
            _simulator = new Simulator(
                _sectorGenerator.Object,
                _simulationRepository.Object,
                _empireGenerator.Object,
                _economicSimulator.Object,
                _militarySimulator.Object,
                _diplomacySimulator.Object,
                _leaderUpdater.Object,
                _deltaApplier.Object);
            _simulationRepository.Setup(x => x.GetSimulation(It.IsAny<ObjectId>()))
                .Returns(new Simulation { EmpireIds = new ObjectId[0] });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(13)]
        public void UpdatesSimulationTicks(int ticks)
        {
            var simulationId = ObjectId.GenerateNewId();
            _simulator.RunForTicks(simulationId, ticks);

            _simulationRepository.Verify(x => x.IncrementTick(simulationId), Times.Exactly(ticks));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(21)]
        public void RunsEconomicSimulationForEachEmpireForEachTick(int ticks)
        {
            var simulation = new Simulation { EmpireIds = new ObjectId[]
            {
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId()
            }};
            _simulationRepository.Setup(x => x.GetSimulation(It.IsAny<ObjectId>()))
                .Returns(simulation);
            
            _simulator.RunForTicks(ObjectId.Empty, ticks);

            foreach (var id in simulation.EmpireIds)
            {
                _economicSimulator.Verify(x => x.RunEmpire(It.IsAny<DeltaMetadata>(), id), Times.Exactly(ticks));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(21)]
        public void RunsLeaderUpdaterForEachTick(int ticks)
        {
            var simulation = new Simulation { EmpireIds = new ObjectId[]
            {
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId()
            }};
            _simulationRepository.Setup(x => x.GetSimulation(It.IsAny<ObjectId>()))
                .Returns(simulation);

            _simulator.RunForTicks(ObjectId.Empty, ticks);

            _leaderUpdater.Verify(x => x.UpdateLeadersForSimulation(It.IsAny<DeltaMetadata>(), simulation), Times.Exactly(ticks));
        }
    }
}