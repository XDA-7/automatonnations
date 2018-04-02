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
        private IMilitaryCalculator _militaryCalculator;

        public EconomicSimulator(IStarSystemRepository starSystemRepository, IEmpireRepository empireRepository, IDevelopmentCalculator developmentCalculator, IMilitaryCalculator militaryCalculator)
        {
            _starSystemRepository = starSystemRepository;
            _empireRepository = empireRepository;
            _developmentCalculator = developmentCalculator;
            _militaryCalculator = militaryCalculator;
        }

        public void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var empire = _empireRepository.GetEmpireSystemsView(empireId);
            ApplyEconomicGrowth(deltaMetadata, empire);
            ApplyMilitaryProduction(deltaMetadata, empire);
        }

        public void ApplyDamage(DeltaMetadata deltaMetadata, EmpireBorderView empireBorderView, double empireDamage, double borderingEmpireDamage)
        {
            var empireDeltas = empireBorderView.EmpireSystems.Select(x => new Delta<double>
            {
                DeltaType = DeltaType.SystemDevelopment,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = x.Id,
                Value = empireDamage
            });
            var borderingDeltas = empireBorderView.BorderingEmpireSystems.Select(x => new Delta<double>
            {
                DeltaType = DeltaType.SystemDevelopment,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = x.Id,
                Value = borderingEmpireDamage
            });
            _starSystemRepository.ApplyDamage(empireDeltas.Concat(borderingDeltas));
        }

        private void ApplyMilitaryProduction(DeltaMetadata deltaMetadata, EmpireSystemsView empire)
        {
            var production = _militaryCalculator.ProductionForEmpire(empire);
            _empireRepository.ApplyMilitaryProduction(deltaMetadata, empire.Empire.Id, production);
        }

        private void ApplyEconomicGrowth(DeltaMetadata deltaMetadata, EmpireSystemsView empire)
        {
            var growthValues = empire.StarSystems
                .SelectMany(x => _developmentCalculator.GrowthFromSystem(x, empire))
                .ToArray();
            var deltas = GetDeltasFromGrowthValues(growthValues, deltaMetadata);
            _starSystemRepository.ApplyDevelopment(deltas);
            ApplyDeltas(empire.StarSystems, deltas);
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