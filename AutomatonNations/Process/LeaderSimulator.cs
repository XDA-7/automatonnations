using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class LeaderSimulator : ILeaderSimulator
    {
        private ILeaderRepository _leaderRepository;
        private IEmpireRepository _empireRepository;
        private IEmpireGenerator _empireGenerator;
        private IRandom _random;

        public LeaderSimulator(
            ILeaderRepository leaderRepository,
            IEmpireRepository empireRepository,
            IEmpireGenerator empireGenerator,
            IRandom random)
        {
            _leaderRepository = leaderRepository;
            _empireRepository = empireRepository;
            _empireGenerator = empireGenerator;
            _random = random;
        }

        public void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var empireView = _empireRepository.GetEmpireSystemsView(empireId);
            var leaders = empireView.Empire.Leaders;
            var leaderCount = leaders.Count();
            if (leaderCount == 0)
            {
                return;
            }

            var randomNumbers = _random.DoubleSet(0.0, 1.0, leaderCount * 2 - 1);
            IncrementSystemLimits(leaders, randomNumbers.Take(leaderCount));
            foreach (var leader in GetShuffledUnderLimitLeaders(leaders))
            {
                var availableSystemIds = GetAvailableSystemIds(leader, empireView);
                if (availableSystemIds.Any())
                {
                    var selectedSystemId = _random.GetRandomElement(availableSystemIds);
                    leader.StarSystemIds = leader.StarSystemIds.Concat(new ObjectId[] { selectedSystemId });
                }
            }

            _leaderRepository.SetLeadersForEmpire(deltaMetadata, empireId, leaders);
            RunSecessions(deltaMetadata, randomNumbers.TakeLast(leaderCount - 1), empireView.Empire);
        }

        private IEnumerable<Leader> GetShuffledUnderLimitLeaders(IEnumerable<Leader> leaders) =>
            _random.ShuffleElements(leaders.Where(leader => leader.SystemLimit > leader.StarSystemIds.Count()));

        private void IncrementSystemLimits(IEnumerable<Leader> leaders, IEnumerable<double> randomNumbers)
        {
            var leaderIncrements = randomNumbers
                .Select(x => x <= Parameters.LeaderSystemLimitIncreaseChancePerTurn)
                .ToArray();
            var i = 0;
            foreach (var leader in leaders)
            {
                if (leaderIncrements[i] && leader.SystemLimit < Parameters.LeaderMaxSystemLimit)
                {
                    leader.SystemLimit++;
                }

                i++;
            }
        }

        private IEnumerable<ObjectId> GetAvailableSystemIds(Leader leader, EmpireSystemsView empireView)
        {
            var leaderControlledSystemIds = GetLeaderControlledSystemIds(empireView);
            if (!leader.StarSystemIds.Any())
            {
                return empireView.Empire.StarSystemsIds
                    .Where(id => !leaderControlledSystemIds.Contains(id));
            }
            else
            {
                var adjacentAvailableIds = empireView.StarSystems
                    .Where(system => leader.StarSystemIds.Contains(system.Id))
                    .SelectMany(system => system.ConnectedSystemIds)
                    .Where(id =>
                        !leaderControlledSystemIds.Contains(id) &&
                        empireView.Empire.StarSystemsIds.Contains(id));
                return adjacentAvailableIds;
            }
        }

        private IEnumerable<ObjectId> GetLeaderControlledSystemIds(EmpireSystemsView empireView) =>
            empireView.Empire.Leaders.SelectMany(x => x.StarSystemIds);
        
        private void RunSecessions(DeltaMetadata deltaMetadata, IEnumerable<double> randomNumbers, Empire empire)
        {
            var secessions = randomNumbers.Select(x => x <= Parameters.LeaderSecessionChancePerTurn).ToArray();
            var i = 0;
            foreach (var leader in empire.Leaders.Where(x => !x.EmpireLeader))
            {
                if (secessions[i])
                {
                    _empireGenerator.CreateForSecedingLeader(deltaMetadata, empire, leader);
                }

                i++;
            }
        }
    }
}