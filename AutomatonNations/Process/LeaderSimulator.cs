using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class LeaderSimulator : ILeaderSimulator
    {
        private ILeaderRepository _leaderRepository;
        private IEmpireRepository _empireRepository;
        private IRandom _random;

        public LeaderSimulator(
            ILeaderRepository leaderRepository,
            IEmpireRepository empireRepository,
            IRandom random)
        {
            _leaderRepository = leaderRepository;
            _empireRepository = empireRepository;
            _random = random;
        }

        public void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var empireView = _empireRepository.GetEmpireSystemsView(empireId);
            var leaders = empireView.Empire.Leaders;
            IncrementSystemLimits(leaders);
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
        }

        private IEnumerable<Leader> GetShuffledUnderLimitLeaders(IEnumerable<Leader> leaders) =>
            _random.ShuffleElements(leaders.Where(leader => leader.SystemLimit > leader.StarSystemIds.Count()));

        private void IncrementSystemLimits(IEnumerable<Leader> leaders)
        {
            var leaderCount = leaders.Count();
            var leaderIncrements = _random.DoubleSet(0.0, 1.0, leaderCount)
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
                var adjacentIds = empireView.StarSystems
                    .Where(system => leader.StarSystemIds.Contains(system.Id))
                    .SelectMany(system => system.ConnectedSystemIds);
                return adjacentIds
                    .Where(id => !leaderControlledSystemIds.Contains(id));
            }
        }

        private IEnumerable<ObjectId> GetLeaderControlledSystemIds(EmpireSystemsView empireView) =>
            empireView.Empire.Leaders.SelectMany(x => x.StarSystemIds);
    }
}