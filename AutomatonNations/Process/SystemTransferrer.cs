using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class SystemTransferrer : ISystemTransferrer
    {
        private IEmpireRepository _empireRepository;
        private ILeaderRepository _leaderRepository;

        public SystemTransferrer(IEmpireRepository empireRepository, ILeaderRepository leaderRepository)
        {
            _empireRepository = empireRepository;
            _leaderRepository = leaderRepository;
        }

        public void TransferSystems(DeltaMetadata deltaMetadata, ObjectId senderId, ObjectId receiverId, IEnumerable<ObjectId> systemIds)
        {
            _empireRepository.TransferSystems(deltaMetadata, senderId, receiverId, systemIds);
            var senderEmpireLeaders = _empireRepository.GetById(senderId).Leaders;
            foreach (var leader in senderEmpireLeaders)
            {
                leader.StarSystemIds = leader.StarSystemIds
                    .Where(systemId => !systemIds.Contains(systemId));
            }

            _leaderRepository.SetLeadersForEmpire(deltaMetadata, senderId, senderEmpireLeaders);
        }
    }
}