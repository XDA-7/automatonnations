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
            ApplyDevelopmentDeltas(simulationView.StarSystems, deltaSet.DeltaDecimals);
            return simulationView;
        }

        private void ApplyDevelopmentDeltas(IEnumerable<StarSystem> starSystems, IEnumerable<Delta<decimal>> decimalDeltas)
        {
            var developmentDeltas = decimalDeltas.Where(x => x.DeltaType == DeltaType.SystemDevelopment);
            foreach (var starSystem in starSystems)
            {
                starSystem.Development -= developmentDeltas
                    .Where(x => x.ReferenceId == starSystem.Id)
                    .Sum(x => x.Value);
            }
        }
    }
}