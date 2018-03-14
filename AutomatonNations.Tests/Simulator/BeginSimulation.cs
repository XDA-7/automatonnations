using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_Simulator
{
    public class BeginSimulation
    {
        private Mock<ISectorGenerator> _sectorGenerator = new Mock<ISectorGenerator>();
        private Mock<ISimulationRepository> _simulationRepository = new Mock<ISimulationRepository>();
        private ISimulator _simulator;

        [Fact]
        public void CreatesNewSimulationWithNewSector()
        {
            var newSectorId = ObjectId.GenerateNewId();
            _sectorGenerator.Setup(x => x.CreateSector(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new CreateSectorResult { SectorId = newSectorId });
            
            _simulator.BeginSimulation();

            _simulationRepository.Verify(x => x.Create(newSectorId), Times.Once);
        }
    }
}