using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class EconomicSimulator : IEconomicSimulator
    {
        private IStarSystemRepository _starSystemRepository;
        private IDevelopmentCalculator _developmentCalculator;

        public EconomicSimulator(IStarSystemRepository starSystemRepository, IDevelopmentCalculator developmentCalculator)
        {
            _starSystemRepository = starSystemRepository;
            _developmentCalculator = developmentCalculator;
        }

        public void RunEmpire(EmpireSystemsView empire)
        {
            var deltas = empire.StarSystems
                .SelectMany(x => GetDevelopmentDeltasForSystem(x, empire))
                .ToArray();
            _starSystemRepository.ApplyDevelopment(deltas);
            ApplyDeltas(empire.StarSystems, deltas);
        }

        private IEnumerable<Delta<decimal>> GetDevelopmentDeltasForSystem(StarSystem starSystem, EmpireSystemsView empireView)
        {
            var connectedSystems = empireView.StarSystems.Where(x => starSystem.ConnectedSystemIds.Contains(x.Id));
            return _developmentCalculator.GrowthFromSystem(starSystem, connectedSystems, empireView.Empire.Alignment.Prosperity);
        }

        private void ApplyDeltas(IEnumerable<StarSystem> starSystems, IEnumerable<Delta<decimal>> deltas)
        {
            foreach (var starSystem in starSystems)
            {
                var systemDeltas = deltas.Where(x => x.ReferenceId == starSystem.Id);
                starSystem.Development += systemDeltas.Sum(x => x.Value);
            }
        }
    }
}