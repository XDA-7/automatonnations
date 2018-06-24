using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_SystemTransferrer
{
    public class TransferSystems
    {
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private Mock<ILeaderRepository> _leaderRepository = new Mock<ILeaderRepository>();
        private ISystemTransferrer _systemTransferrer;

        private ObjectId _sendingEmpireId = ObjectId.GenerateNewId();
        private ObjectId[] _sendingEmpireSystemIds = new ObjectId[]
        {
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId()
        };
        private Empire _sendingEmpire;

        private ObjectId _receivingEmpireId = ObjectId.GenerateNewId();
        private ObjectId[] _receivingEmpireSystemIds = new ObjectId[]
        {
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId()
        };
        private Empire _receivingEmpire;

        public TransferSystems()
        {
            _systemTransferrer = new SystemTransferrer(_empireRepository.Object, _leaderRepository.Object);
            _sendingEmpire = new Empire
            {
                Id = _sendingEmpireId,
                StarSystemsIds = _sendingEmpireSystemIds
            };
            _receivingEmpire = new Empire
            {
                Id = _receivingEmpireId,
                StarSystemsIds = _receivingEmpireSystemIds
            };

            _sendingEmpire.Leaders = new Leader[]
            {
                new Leader { StarSystemIds = new ObjectId[] { _sendingEmpireSystemIds[0], _sendingEmpireSystemIds[1], _sendingEmpireSystemIds[3] } }
            };
            _receivingEmpire.Leaders = new Leader[0];

            _empireRepository
                .Setup(x => x.GetById(_sendingEmpireId))
                .Returns(_sendingEmpire);
            _empireRepository
                .Setup(x => x.GetById(_receivingEmpireId))
                .Returns(_receivingEmpire);
        }

        [Fact]
        public void CallsEmpireRepositoryToTransferSystems()
        {
            _systemTransferrer.TransferSystems(
                It.IsAny<DeltaMetadata>(),
                _sendingEmpireId,
                _receivingEmpireId,
                new ObjectId[] { _sendingEmpireSystemIds[1], _sendingEmpireSystemIds[3] });
            _empireRepository
                .Verify(
                    x => x.TransferSystems(
                        It.IsAny<DeltaMetadata>(),
                        _sendingEmpireId,
                        _receivingEmpireId,
                        It.Is<IEnumerable<ObjectId>>(
                            systems =>
                            systems.Count() == 2 &&
                            systems.Contains(_sendingEmpireSystemIds[1]) &&
                            systems.Contains(_sendingEmpireSystemIds[3]))),
                    Times.Once);
        }

        [Fact]
        public void UpdatesLeadersOfSenderToNoLongerHoldTransferredSystems()
        {
            _systemTransferrer.TransferSystems(
                It.IsAny<DeltaMetadata>(),
                _sendingEmpireId,
                _receivingEmpireId,
                new ObjectId[] { _sendingEmpireSystemIds[1], _sendingEmpireSystemIds[3] });
            _leaderRepository
                .Verify(
                    x => x.SetLeadersForEmpire(
                        It.IsAny<DeltaMetadata>(),
                        _sendingEmpireId,
                        It.Is<IEnumerable<Leader>>(
                            leaders =>
                            leaders.Single().StarSystemIds.Count() == 1 &&
                            leaders.Single().StarSystemIds.Single() == _sendingEmpireSystemIds[0])),
                    Times.Once);
        }
    }
}