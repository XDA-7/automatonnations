using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class LeaderGenerator : ILeaderGenerator
    {
        private ILeaderRepository _leaderRepository;
        private IEmpireRepository _empireRepository;
        private IRandom _random;

        public LeaderGenerator(ILeaderRepository leaderRepository, IEmpireRepository empireRepository, IRandom random)
        {
            _leaderRepository = leaderRepository;
            _empireRepository = empireRepository;
            _random = random;
        }

        public void GenerateLeadersForEmpire(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var empire = _empireRepository.GetById(empireId);
            var leaderCreationAttempts = _random.DoubleSet(0.0, 1.0, empire.StarSystemsIds.Count());
            var createdLeaders = leaderCreationAttempts.Where(x => x < Parameters.LeaderCreationChancePerSystemPerTick).Count();
            CreateLeaders(deltaMetadata, empireId, createdLeaders, empire.Leaders);
        }

        private void CreateLeaders(DeltaMetadata deltaMetadata, ObjectId empireId, int count, IEnumerable<Leader> existingLeaders)
        {
            var empireLeaderExists = existingLeaders.Any(leader => leader.EmpireLeader);
            var newLeaders = new Leader[count];
            var incomeRateBonuses = _random.DoubleSet(
                Parameters.LeaderMinimumDevelopmentBonus,
                Parameters.LeaderMaximumDevelopmentBonus,
                count);
            var militaryWitholdingRates = _random.DoubleSet(
                Parameters.LeaderMinimumMilitaryWitholdingRate,
                Parameters.LeaderMaximumMilitaryWitholdingRate,
                count);
            for (var i = 0; i < count; i++)
            {
                newLeaders[i] = new Leader
                {
                    SystemLimit = Parameters.LeaderInitialSystemLimit,
                    IncomeRateBonus = incomeRateBonuses[i],
                    MilitaryWitholdingRate = militaryWitholdingRates[i],
                    EmpireLeader = !empireLeaderExists && i == 0,
                    StarSystemIds = new ObjectId[0]
                };
            }

            if (newLeaders.Length != 0)
            {
                _leaderRepository.SetLeadersForEmpire(deltaMetadata, empireId, existingLeaders.Concat(newLeaders));
            }
        }
    }
}