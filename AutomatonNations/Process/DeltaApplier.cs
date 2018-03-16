using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class DeltaApplier : IDeltaApplier
    {
        private IDeltaRepository _deltaRepository;
        private ISimulationRepository _simulationRepository;

        public DeltaApplier(IDeltaRepository deltaRepository, ISimulationRepository simulationRepository)
        {
            _deltaRepository = deltaRepository;
            _simulationRepository = simulationRepository;
        }

        public SimulationView GetForTick(ObjectId simulationId, int tick)
        {
            var simulationView = _simulationRepository.GetSimulationView(simulationId);
            var deltaSet = _deltaRepository.GetForSimulation(simulationId, tick, simulationView.Ticks);
            simulationView.StarSystems = ApplyDevelopmentDeltas(simulationView.StarSystems, deltaSet.DeltaDoubles);
            return simulationView;
        }

        private IEnumerable<StarSystem> ApplyDevelopmentDeltas(IEnumerable<StarSystem> starSystems, IEnumerable<Delta<double>> deltas)
        {
            var developmentDeltas = deltas.Where(x => x.DeltaType == DeltaType.SystemDevelopment);
            return starSystems.Select(starSystem =>
            {
                starSystem.Development -= developmentDeltas
                    .Where(delta => delta.ReferenceId == starSystem.Id)
                    .Sum(delta => delta.Value);
                return starSystem;
            });
        }
    }
}