using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class MilitarySimulator : IMilitarySimulator
    {
        private IMilitaryCalculator _militaryCalculator;
        private IWarRepository _warRepository;
        private IEconomicSimulator _economicSimulator;
        private IEmpireRepository _empireRepository;

        public MilitarySimulator(IMilitaryCalculator militaryCalculator, IWarRepository warRepository, IEconomicSimulator economicSimulator, IEmpireRepository empireRepository)
        {
            _militaryCalculator = militaryCalculator;
            _warRepository = warRepository;
            _economicSimulator = economicSimulator;
            _empireRepository = empireRepository;
        }

        public void Run(DeltaMetadata deltaMetadata, ObjectId simulationId)
        {
            var wars = _warRepository.GetWars(simulationId);
            foreach (var war in wars)
            {
                RunWar(deltaMetadata, war);
            }
        }

        private void RunWar(DeltaMetadata deltaMetadata, War war)
        {   
            var borderView = _empireRepository.GetEmpireBorderView(war.AttackerId, war.DefenderId);
            var attacker = borderView.Empire;
            var defender = borderView.BorderingEmpire;
            var combatResult = _militaryCalculator.Combat(attacker, defender);
            _warRepository.ContinueWar(
                deltaMetadata,
                war.Id,
                combatResult.AttackerDamage.MilitaryDamage,
                combatResult.DefenderDamage.MilitaryDamage);
            _economicSimulator.ApplyDamage(
                deltaMetadata,
                borderView,
                combatResult.AttackerDamage.CollateralDamage,
                combatResult.DefenderDamage.CollateralDamage);
            _empireRepository.ApplyMilitaryDamage(
                deltaMetadata,
                war.AttackerId,
                combatResult.DefenderDamage.MilitaryDamage);
            _empireRepository.ApplyMilitaryDamage(
                deltaMetadata,
                war.DefenderId,
                combatResult.AttackerDamage.MilitaryDamage);
            SystemTransfer(deltaMetadata, combatResult.TerritoryGain, borderView);
            TryEndWar(deltaMetadata, war.Id, combatResult.TerritoryGain, war.AttackerId, war.DefenderId);
        }

        private void SystemTransfer(DeltaMetadata deltaMetadata, TerritoryGain territory, EmpireBorderView empireBorderView)
        {
            if (territory == TerritoryGain.Attacker)
            {
                _empireRepository.TransferSystems(
                    deltaMetadata,
                    empireBorderView.BorderingEmpire.Id,
                    empireBorderView.Empire.Id,
                    empireBorderView.BorderingEmpireSystems.Select(x => x.Id));
            }
            else if (territory == TerritoryGain.Defender)
            {
                _empireRepository.TransferSystems(
                    deltaMetadata,
                    empireBorderView.Empire.Id,
                    empireBorderView.BorderingEmpire.Id,
                    empireBorderView.EmpireSystems.Select(x => x.Id));
            }
        }

        private void TryEndWar(DeltaMetadata deltaMetadata, ObjectId warId, TerritoryGain territoryGain, ObjectId attackerId, ObjectId defenderId)
        {
            if (territoryGain == TerritoryGain.Attacker && AllSystemsLost(defenderId))
            {
                _warRepository.EndWarsWithParticipant(deltaMetadata, defenderId);
                _empireRepository.EmpireDefeated(deltaMetadata, defenderId);
            }
            else if (territoryGain == TerritoryGain.Defender && AllSystemsLost(attackerId))
            {
                _warRepository.EndWarsWithParticipant(deltaMetadata, attackerId);
                _empireRepository.EmpireDefeated(deltaMetadata, attackerId);
            }
        }

        private bool AllSystemsLost(ObjectId empireId)
        {
            var empire = _empireRepository.GetById(empireId);
            return !empire.StarSystemsIds.Any();
        }
    }
}