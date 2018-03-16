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

        public EmpireGenerator(IRandom random, IEmpireRepository empireRepository)
        {
            _random = random;
            _empireRepository = empireRepository;
        }

        public IEnumerable<ObjectId> CreatePerSystem(int starSystemCount, IEnumerable<ObjectId> starSystemIds)
        {
            var alignments = CreateRandomAlignments(starSystemCount);
            var requests = CreateRequestPerSystem(starSystemIds, alignments);
            return _empireRepository.Create(requests);
        }

        private IEnumerable<CreateEmpireRequest> CreateRequestPerSystem(IEnumerable<ObjectId> systemIds, IEnumerable<Alignment> alignments) =>
            systemIds.Zip(alignments, (systemId, alignment) => new CreateEmpireRequest(alignment, new ObjectId[] { systemId }));

        private IEnumerable<Alignment> CreateRandomAlignments(int count)
        {
            var values = _random.NextSet(_alignmentRange, count);
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