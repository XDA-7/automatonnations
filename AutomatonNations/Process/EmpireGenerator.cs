using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class EmpireGenerator : IEmpireGenerator
    {
        private const int _alignmentRange = 100;
        private const double _alignmentRangeDouble = (double)_alignmentRange;
        private IRandom _random;
        private IEmpireRepository _empireRepository;
        private ILeaderRepository _leaderRepository;
        private ISystemTransferrer _systemTransferrer;

        public EmpireGenerator(IRandom random, IEmpireRepository empireRepository, ILeaderRepository leaderRepository, ISystemTransferrer systemTransferrer)
        {
            _random = random;
            _empireRepository = empireRepository;
            _leaderRepository = leaderRepository;
            _systemTransferrer = systemTransferrer;
        }

        public IEnumerable<ObjectId> CreatePerSystem(int starSystemCount, IEnumerable<ObjectId> starSystemIds)
        {
            var alignments = CreateRandomAlignments(starSystemCount);
            var requests = CreateRequestPerSystem(starSystemIds, alignments);
            return _empireRepository.Create(requests);
        }

        public void CreateForSecedingLeader(DeltaMetadata deltaMetadata, Empire empire, Leader leader)
        {
            var secedingEmpireId = _empireRepository.Create(deltaMetadata, new CreateEmpireRequest[]
            {
                new CreateEmpireRequest(
                    new Alignment { Power = empire.Alignment.Power, Prosperity = empire.Alignment.Prosperity },
                    new ObjectId[0])
            })
            .Single();
            _systemTransferrer.TransferSystems(deltaMetadata, empire.Id, secedingEmpireId, leader.StarSystemIds);
            leader.EmpireLeader = true;
            _leaderRepository.SetLeadersForEmpire(deltaMetadata, secedingEmpireId, new Leader[] { leader });
            _leaderRepository.SetLeadersForEmpire(deltaMetadata, empire.Id, empire.Leaders.Where(empireLeader => empireLeader != leader));
        }

        private IEnumerable<CreateEmpireRequest> CreateRequestPerSystem(IEnumerable<ObjectId> systemIds, IEnumerable<Alignment> alignments) =>
            systemIds.Zip(alignments, (systemId, alignment) => new CreateEmpireRequest(alignment, new ObjectId[] { systemId }));

        private IEnumerable<Alignment> CreateRandomAlignments(int count)
        {
            var values = _random.IntegerSet(_alignmentRange, count);
            return values.Select(x => CreateAlignment(x));
        }

        private Alignment CreateAlignment(int prosperityValue)
        {
            var prosperityValueDouble = (double)prosperityValue;
            return new Alignment
            {
                Prosperity = prosperityValueDouble / _alignmentRangeDouble,
                Power = (_alignmentRangeDouble - prosperityValueDouble) / _alignmentRangeDouble
            };
        }
    }
}