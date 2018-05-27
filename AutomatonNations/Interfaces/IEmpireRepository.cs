using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IEmpireRepository
    {
        IEnumerable<ObjectId> Create(IEnumerable<CreateEmpireRequest> requests);

        IEnumerable<ObjectId> Create(DeltaMetadata deltaMetadata, IEnumerable<CreateEmpireRequest> requests);

        Empire GetById(ObjectId empireId);

        EmpireSystemsView GetEmpireSystemsView(ObjectId empireId);

        IEnumerable<EmpireSystemsView> GetEmpireSystemsViews(IEnumerable<ObjectId> empireIds);

        IEnumerable<EmpireBorderView> GetEmpireBorderViews(ObjectId empireId);

        EmpireBorderView GetEmpireBorderView(ObjectId empireId, ObjectId borderingEmpireId);

        void TransferSystems(DeltaMetadata deltaMetadata, ObjectId senderId, ObjectId receiverId, IEnumerable<ObjectId> systemIds);

        void ApplyMilitaryDamage(DeltaMetadata deltaMetadata, ObjectId empireId, double damage);

        void ApplyMilitaryProduction(DeltaMetadata deltaMetadata, ObjectId empireId, double increase);

        void EmpireDefeated(DeltaMetadata deltaMetadata, ObjectId empireId);
    }
}