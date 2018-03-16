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
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private Mock<IEconomicSimulator> _economicSimulator = new Mock<IEconomicSimulator>();
        private Mock<IDeltaApplier> _deltaApplier = new Mock<IDeltaApplier>();
        private ISimulator _simulator;

        public RunForTicks()
        {
            _simulator = new Simulator(_sectorGenerator.Object, _simulationRepository.Object, _empireGenerator.Object, _empireRepository.Object, _economicSimulator.Object, _deltaApplier.Object);
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

            _simulationRepository.Verify(x => x.IncrementTicks(simulationId, ticks), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(21)]
        public void RunsEconomicSimulationForEachEmpireForEachTick(int ticks)
        {
            var empireSystemsViews = new EmpireSystemsView[]
            {
                new EmpireSystemsView(),
                new EmpireSystemsView(),
                new EmpireSystemsView(),
                new EmpireSystemsView(),
                new EmpireSystemsView()
            };
            _empireRepository.Setup(x => x.GetEmpireSystemsViews(It.IsAny<IEnumerable<ObjectId>>()))
                .Returns(empireSystemsViews);
            
            _simulator.RunForTicks(ObjectId.Empty, ticks);

            foreach (var empireSystemsView in empireSystemsViews)
            {
                _economicSimulator.Verify(x => x.RunEmpire(It.IsAny<DeltaMetadata>(), empireSystemsView), Times.Exactly(ticks));
            }
        }
    }
}