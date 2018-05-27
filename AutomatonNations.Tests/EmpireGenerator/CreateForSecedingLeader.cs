using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_EmpireGenerator
{
    public class CreateForSecedingLeader
    {
        private Mock<IRandom> _random = new Mock<IRandom>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private Mock<ILeaderRepository> _leaderRepository = new Mock<ILeaderRepository>();
        private IEmpireGenerator _empireGenerator;

        private ObjectId[] _systemIds = new ObjectId[]
        {
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId()
        };
        private Alignment _alignment = new Alignment
        {
            Power = 33.5,
            Prosperity = 66.5
        };
        private Leader[] _leaders;
        private Empire _empire;
        private ObjectId _secedingEmpireId = ObjectId.GenerateNewId();

        public CreateForSecedingLeader()
        {
            _empireGenerator = new EmpireGenerator(_random.Object, _empireRepository.Object, _leaderRepository.Object);
            _leaders = new Leader[]
            {
                new Leader { StarSystemIds = new ObjectId[] { _systemIds[0], _systemIds[1] } },
                new Leader { StarSystemIds = new ObjectId[] { _systemIds[2], _systemIds[3] } },
                new Leader { StarSystemIds = new ObjectId[] { _systemIds[4] } }
            };
            _empire = new Empire
            {
                Id = ObjectId.GenerateNewId(),
                Alignment = _alignment,
                StarSystemsIds = _systemIds,
                Leaders = _leaders
            };
            _empireRepository
                .Setup(x => x.Create(It.IsAny<DeltaMetadata>(), It.IsAny<IEnumerable<CreateEmpireRequest>>()))
                .Returns(new ObjectId[] { _secedingEmpireId });
        }

        [Fact]
        public void CreatesNewEmpireWithSecedingLeaderAsEmpireLeader()
        {
            _empireGenerator.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[0]);
            _empireRepository.Verify(x => x.Create(It.IsAny<DeltaMetadata>(), It.Is<IEnumerable<CreateEmpireRequest>>(request => request.Count() == 1)), Times.Once);
            _leaderRepository.Verify(
                x => x.SetLeadersForEmpire(
                    It.IsAny<DeltaMetadata>(),
                    _secedingEmpireId,
                    It.Is<IEnumerable<Leader>>(
                        leader =>
                        leader.Single() == _leaders[0] &&
                        leader.Single().EmpireLeader)),
                Times.Once);
        }

        [Fact]
        public void TransfersAllSystemsControlledByLeaderToNewEmpire()
        {
            _empireGenerator.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[0]);
            _empireRepository.Verify(
                x => x.TransferSystems(It.IsAny<DeltaMetadata>(), _empire.Id, _secedingEmpireId, _leaders[0].StarSystemIds),
                Times.Once);
        }

        [Fact]
        public void RemovesSecedingLeaderFromEmpire()
        {
            _empireGenerator.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[0]);
            _leaderRepository.Verify(
                x => x.SetLeadersForEmpire(
                    It.IsAny<DeltaMetadata>(),
                    _empire.Id,
                    It.Is<IEnumerable<Leader>>(
                        leaders =>
                        leaders.Count() == 2 &&
                        leaders.ToArray()[0] == _leaders[1] &&
                        leaders.ToArray()[1] == _leaders[2])),
                Times.Once);
        }

        [Fact]
        public void InheritsAlignmentFromSecededEmpire()
        {
            _empireGenerator.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[0]);
            _empireRepository.Verify(
                x => x.Create(
                    It.IsAny<DeltaMetadata>(),
                    It.Is<IEnumerable<CreateEmpireRequest>>(
                        request =>
                        request.Single().Alignment.Power == _alignment.Power &&
                        request.Single().Alignment.Prosperity == _alignment.Prosperity)),
                Times.Once);
        }

        [Fact]
        public void CreatesSecedingEmpireWithNoSystemsInitially()
        {
            _empireGenerator.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[0]);
            _empireRepository.Verify(
                x => x.Create(
                    It.IsAny<DeltaMetadata>(),
                    It.Is<IEnumerable<CreateEmpireRequest>>(
                        request =>
                        !request.Single().StarSystemIds.Any())),
                Times.Once);
        }
    }
}