using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ISystemTransferrer
    {
        void TransferSystems(DeltaMetadata deltaMetadata, ObjectId senderId, ObjectId receiverId, IEnumerable<ObjectId> systemIds);
    }
}