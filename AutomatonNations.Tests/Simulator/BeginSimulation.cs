using System.Collections.Generic;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_Simulator
{
    public class BeginSimulation
    {
        private Mock<ISectorGenerator> _sectorGenerator = new Mock<ISectorGenerator>();
        private Mock<ISimulationRepository> _simulationRepository = new Mock<ISimulationRepository>();
        private Mock<IEmpireGenerator> _empireGenerator = new Mock<IEmpireGenerator>();
        private ISimulator _simulator;

        public BeginSimulation()
        {
            _simulator = new Simulator(_sectorGenerator.Object, _simulationRepository.Object, _empireGenerator.Object);
        }

        [Fact]
        public void CreatesNewSimulationWithNewSector()
        {
            var newSectorId = ObjectId.GenerateNewId();
            _sectorGenerator.Setup(x => x.CreateSector(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new CreateSectorResult(newSectorId, new StarSystem[0]));
            
            _simulator.BeginSimulation(new BeginSimulationRequest(0, 0, 0));

            _simulationRepository.Verify(x => x.Create(newSectorId, It.IsAny<IEnumerable<ObjectId>>()), Times.Once);
        }

        [Fact]
        public void CreatesEmpirePerSystem()
        {
            _sectorGenerator.Setup(x => x.CreateSector(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, new StarSystem[0]));
            _simulator.BeginSimulation(new BeginSimulationRequest(22, 0, 0));

            _empireGenerator.Verify(x => x.CreatePerSystem(22, It.IsAny<IEnumerable<ObjectId>>()), Times.Once);
        }
    }
}