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
            CreateLeaders(deltaMetadata, empireId, createdLeaders);
        }

        private void CreateLeaders(DeltaMetadata deltaMetadata, ObjectId empireId, int count)
        {
            var leaders = new Leader[count];
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
                leaders[i] = new Leader
                {
                    SystemLimit = Parameters.LeaderInitialSystemLimit,
                    IncomeRateBonus = incomeRateBonuses[i],
                    MilitaryWitholdingRate = militaryWitholdingRates[i]
                };
            }

            _leaderRepository.SetLeadersForEmpire(deltaMetadata, empireId, leaders);
        }
    }
}