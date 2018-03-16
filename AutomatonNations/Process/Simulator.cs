using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class Simulator : ISimulator
    {
        private ISectorGenerator _sectorGenerator;
        private ISimulationRepository _simulationRepository;
        private IEmpireGenerator _empireGenerator;
        private IEmpireRepository _empireRepository;
        private IEconomicSimulator _economicSimulator;
        private IDeltaApplier _deltaApplier;

        public Simulator(ISectorGenerator sectorGenerator, ISimulationRepository simulationRepository, IEmpireGenerator empireGenerator, IEmpireRepository empireRepository, IEconomicSimulator economicSimulator, IDeltaApplier deltaApplier)
        {
            _sectorGenerator = sectorGenerator;
            _simulationRepository = simulationRepository;
            _empireGenerator = empireGenerator;
            _empireRepository = empireRepository;
            _economicSimulator = economicSimulator;
            _deltaApplier = deltaApplier;
        }

        public ObjectId BeginSimulation(BeginSimulationRequest request)
        {
            var sector = _sectorGenerator.CreateSector(request.SectorStarCount, request.SectorSize, request.SystemConnectivityRadius, request.BaseDevelopment);
            var systemIds = sector.StarSystems.Select(x => x.Id);
            var empireIds = _empireGenerator.CreatePerSystem(request.SectorStarCount, systemIds);
            return _simulationRepository.Create(sector.SectorId, empireIds);
        }

        public void RunForTicks(ObjectId simulationId, int ticks)
        {
            var simulation = _simulationRepository.GetSimulation(simulationId);
            var empireSystemViews = _empireRepository.GetEmpireSystemsViews(simulation.EmpireIds);
            for (var i = 0; i < ticks; i++)
            {
                var deltaMetadata = new DeltaMetadata(simulationId, simulation.Ticks + i + 1);
                RunForTick(empireSystemViews, deltaMetadata);
            }

            _simulationRepository.IncrementTicks(simulationId, ticks);
        }

        public SimulationView GetLatest(ObjectId simulationId) =>
            _simulationRepository.GetSimulationView(simulationId);

        public SimulationView GetAtTick(ObjectId simulationId, int tick) =>
            _deltaApplier.GetForTick(simulationId, tick);

        private void RunForTick(IEnumerable<EmpireSystemsView> empireSystemsViews, DeltaMetadata deltaMetadata)
        {
            foreach (var empireSystemsView in empireSystemsViews)
            {
                _economicSimulator.RunEmpire(deltaMetadata, empireSystemsView);
            }
        }
    }
}