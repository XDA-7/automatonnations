using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class EconomicSimulator : IEconomicSimulator
    {
        private IStarSystemRepository _starSystemRepository;
        private IEmpireRepository _empireRepository;
        private IDevelopmentCalculator _developmentCalculator;

        public EconomicSimulator(IStarSystemRepository starSystemRepository, IEmpireRepository empireRepository, IDevelopmentCalculator developmentCalculator)
        {
            _starSystemRepository = starSystemRepository;
            _empireRepository = empireRepository;
            _developmentCalculator = developmentCalculator;
        }

        public void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var empire = _empireRepository.GetEmpireSystemsView(empireId);
            var growthValues = empire.StarSystems
                .SelectMany(x => GetGrowthFromSystem(x, empire))
                .ToArray();
            var deltas = GetDeltasFromGrowthValues(growthValues, deltaMetadata);
            _starSystemRepository.ApplyDevelopment(deltas);
            ApplyDeltas(empire.StarSystems, deltas);
        }

        public void ApplyDamage(DeltaMetadata deltaMetadata, EmpireBorderView empireBorderView, double empireDamage, double borderingEmpireDamage)
        {
            var empireDeltas = empireBorderView.EmpireSystems.Select(x => new Delta<double>
            {
                DeltaType = DeltaType.SystemDevelopment,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = x.Id,
                Value = -empireDamage
            });
            var borderingDeltas = empireBorderView.BorderingEmpireSystems.Select(x => new Delta<double>
            {
                DeltaType = DeltaType.SystemDevelopment,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = x.Id,
                Value = -borderingEmpireDamage
            });
            _starSystemRepository.ApplyDevelopment(empireDeltas.Concat(borderingDeltas));
        }

        private IEnumerable<Delta<double>> GetDeltasFromGrowthValues(IEnumerable<GrowthFromSystemResult> values, DeltaMetadata metadata) =>
            values.Select(x => new Delta<double>
            {
                DeltaType = DeltaType.SystemDevelopment,
                SimulationId = metadata.SimulationId,
                Tick = metadata.Tick,
                Value = x.Growth,
                ReferenceId = x.SystemId
            });

        private IEnumerable<GrowthFromSystemResult> GetGrowthFromSystem(StarSystem starSystem, EmpireSystemsView empireView)
        {
            var connectedSystems = empireView.StarSystems.Where(x => starSystem.ConnectedSystemIds.Contains(x.Id));
            return _developmentCalculator.GrowthFromSystem(starSystem, connectedSystems, empireView.Empire.Alignment.Prosperity);
        }

        private void ApplyDeltas(IEnumerable<StarSystem> starSystems, IEnumerable<Delta<double>> deltas)
        {
            foreach (var starSystem in starSystems)
            {
                var systemDeltas = deltas.Where(x => x.ReferenceId == starSystem.Id);
                starSystem.Development += systemDeltas.Sum(x => x.Value);
            }
        }
    }
}