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
        private IEconomicSimulator _economicSimulator;
        private IMilitarySimulator _militarySimulator;
        private IDiplomacySimulator _diplomacySimulator;
        private IDeltaApplier _deltaApplier;

        public Simulator(
            ISectorGenerator sectorGenerator,
            ISimulationRepository simulationRepository,
            IEmpireGenerator empireGenerator,
            IEconomicSimulator economicSimulator,
            IMilitarySimulator militarySimulator,
            IDiplomacySimulator diplomacySimulator,
            IDeltaApplier deltaApplier)
        {
            _sectorGenerator = sectorGenerator;
            _simulationRepository = simulationRepository;
            _empireGenerator = empireGenerator;
            _economicSimulator = economicSimulator;
            _militarySimulator = militarySimulator;
            _diplomacySimulator = diplomacySimulator;
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
            for (var i = 0; i < ticks; i++)
            {
                var deltaMetadata = new DeltaMetadata(simulationId, simulation.Ticks + i + 1);
                RunForTick(simulation, deltaMetadata);
                _simulationRepository.IncrementTick(simulationId);
            }
        }

        public SimulationView GetLatest(ObjectId simulationId) =>
            _simulationRepository.GetSimulationView(simulationId);

        public SimulationView GetAtTick(ObjectId simulationId, int tick) =>
            _deltaApplier.GetForTick(simulationId, tick);

        private void RunForTick(Simulation simulation, DeltaMetadata deltaMetadata)
        {
            _militarySimulator.Run(deltaMetadata, simulation.Id);
            foreach (var id in simulation.EmpireIds)
            {
                _economicSimulator.RunEmpire(deltaMetadata, id);
                _diplomacySimulator.RunEmpire(deltaMetadata, id);
            }
        }
    }
}