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
            var deltaSet = _deltaRepository.GetForSimulation(simulationId, tick, simulationView.Simulation.Ticks);
            simulationView.StarSystems = ApplyDevelopmentDeltas(simulationView.StarSystems, deltaSet.DeltaDoubles);
            simulationView.Empires = ApplyMilitaryDeltas(simulationView.Empires, deltaSet.DeltaDoubles);
            simulationView.Empires = ApplySystemTransferDeltas(simulationView.Empires, deltaSet.DeltaObjectIds);
            simulationView.Wars = ApplyWarDeltas(simulationView.Wars, deltaSet.Deltas);
            simulationView.Wars = ApplyWarDamageDeltas(simulationView.Wars, deltaSet.DeltaDoubles);
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

        private IEnumerable<Empire> ApplySystemTransferDeltas(IEnumerable<Empire> empires, IEnumerable<Delta<ObjectId>> deltas)
        {
            var gainDeltas = deltas.Where(x => x.DeltaType == DeltaType.EmpireSystemGain);
            var lossDeltas = deltas.Where(x => x.DeltaType == DeltaType.EmpireSystemLoss);
            return empires.Select(empire => {
                var empireGains = gainDeltas
                    .Where(x => x.ReferenceId == empire.Id)
                    .Select(x => x.Value);
                var empireLosses = lossDeltas
                    .Where(x => x.ReferenceId == empire.Id)
                    .Select(x => x.Value);
                empire.StarSystemsIds = empire.StarSystemsIds
                    .Where(system => !empireGains.Contains(system))
                    .Concat(empireLosses);
                return empire;
            });
        }

        private IEnumerable<Empire> ApplyMilitaryDeltas(IEnumerable<Empire> empires, IEnumerable<Delta<double>> deltas)
        {
            var militaryDeltas = deltas.Where(x => x.DeltaType == DeltaType.EmpireMilitary);
            return empires.Select(empire =>
            {
                empire.Military -= militaryDeltas
                    .Where(x => x.ReferenceId == empire.Id)
                    .Select(x => x.Value)
                    .Sum();
                return empire;
            });
        }

        private IEnumerable<War> ApplyWarDeltas(IEnumerable<War> wars, IEnumerable<Delta> deltas)
        {
            var beginDeltaIds = deltas
                .Where(x => x.DeltaType == DeltaType.WarBegin)
                .Select(x => x.ReferenceId);
            var endDeltaIds = deltas
                .Where(x => x.DeltaType == DeltaType.WarEnd)
                .Select(x => x.ReferenceId);
            return wars
                .Where(x => !beginDeltaIds.Contains(x.Id))
                .Select(x =>
                {
                    if (endDeltaIds.Contains(x.Id))
                    {
                        x.Active = true;
                    }

                    return x;
                });
        }

        private IEnumerable<War> ApplyWarDamageDeltas(IEnumerable<War> wars, IEnumerable<Delta<double>> deltas)
        {
            var attackerDamageDeltas = deltas.Where(x => x.DeltaType == DeltaType.WarAttackerDamage);
            var defenderDamageDeltas = deltas.Where(x => x.DeltaType == DeltaType.WarDefenderDamage);
            return wars.Select(war =>
            {
                war.AttackerDamage -= attackerDamageDeltas
                    .Where(x => x.ReferenceId == war.Id)
                    .Select(x => x.Value)
                    .Sum();
                war.DefenderDamage -= defenderDamageDeltas
                    .Where(x => x.ReferenceId == war.Id)
                    .Select(x => x.Value)
                    .Sum();
                return war;
            });
        }
    }
}