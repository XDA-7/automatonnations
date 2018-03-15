using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class Simulator : ISimulator
    {
        private ISectorGenerator _sectorGenerator;
        private ISimulationRepository _simulationRepository;
        private IEmpireGenerator _empireGenerator;

        public Simulator(ISectorGenerator sectorGenerator, ISimulationRepository simulationRepository, IEmpireGenerator empireGenerator)
        {
            _sectorGenerator = sectorGenerator;
            _simulationRepository = simulationRepository;
            _empireGenerator = empireGenerator;
        }

        public ObjectId BeginSimulation(BeginSimulationRequest request)
        {
            var sector = _sectorGenerator.CreateSector(request.SectorStarCount, request.SectorSize, request.SystemConnectivityRadius);
            var systemIds = sector.StarSystems.Select(x => x.Id);
            var empireIds = _empireGenerator.CreatePerSystem(request.SectorStarCount, systemIds);
            return _simulationRepository.Create(sector.SectorId, empireIds);
        }

        public void RunForTicks(ObjectId simulationId, int ticks)
        {
        }

        public SimulationView GetLatest(ObjectId simulationId)
        {
            return null;
        }

        public SimulationView GetAtTick(ObjectId simulationId, int tick)
        {
            return null;
        }
    }
}