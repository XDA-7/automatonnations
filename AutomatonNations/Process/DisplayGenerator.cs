using AutomatonNations.Presentation;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class DisplayGenerator : IDisplayGenerator
    {
        private ISimulator _simulator;
        private IPresentationRepository _presentationRepository;

        public DisplayGenerator(ISimulator simulator, IPresentationRepository presentationRepository)
        {
            _simulator = simulator;
            _presentationRepository = presentationRepository;
        }

        public void CreateForSimulation(ObjectId simulationId)
        {
            var simulation = _simulator.GetLatest(simulationId);
            var lastTick = simulation.Simulation.Ticks;
            CreateForSimulation(simulationId, 1, lastTick);
        }

        public void CreateForSimulation(ObjectId simulationId, int startTick, int endTick)
        {
            for (var i = startTick; i <= endTick; i++)
            {
                var simulation = _simulator.GetAtTick(simulationId, i);
                CreateForTick(simulation, i);
            }
        }

        private void CreateForTick(SimulationView simulationView, int tick) =>
            _presentationRepository.Create(new Presentation.Sector
            {
                SimulationId = simulationView.Simulation.Id,
                Tick = tick,
                StarSystems = GetStarSystems(simulationView),
                StarSystemConnections = GetStarSystemConnections(simulationView.StarSystems)
            });

        private IEnumerable<StarSystemConnection> GetStarSystemConnections(IEnumerable<StarSystem> starSystems)
        {
            var reference = starSystems.ToDictionary(system => system.Id);
            return starSystems.SelectMany(
                system => system.ConnectedSystemIds.Select(
                    id => new StarSystemConnection
                    {
                        Source = system.Coordinate,
                        Destination = reference[id].Coordinate
                    }));
        }

        private IEnumerable<Presentation.StarSystem> GetStarSystems(SimulationView simulationView)
        {
            var controllingEmpires = GetControllingEmpires(simulationView);
            return simulationView.StarSystems.Select(system => new Presentation.StarSystem
            {
                Coordinate = system.Coordinate,
                Development = system.Development,
                EmpireIds = controllingEmpires[system.Id]
            });
        }

        private Dictionary<ObjectId, List<ObjectId>> GetControllingEmpires(SimulationView simulationView)
        {
            var result = new Dictionary<ObjectId, List<ObjectId>>();
            foreach (var system in simulationView.StarSystems)
            {
                result.Add(system.Id, new List<ObjectId>());
            }

            foreach (var empire in simulationView.Empires)
            {
                foreach (var systemId in empire.StarSystemsIds)
                {
                    result[systemId].Add(empire.Id);
                }
            }

            return result;
        }
    }
}