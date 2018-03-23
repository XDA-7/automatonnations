using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IEmpireRepository
    {
        IEnumerable<ObjectId> Create(IEnumerable<CreateEmpireRequest> requests);

        IEnumerable<EmpireSystemsView> GetEmpireSystemsViews(IEnumerable<ObjectId> empireIds);

        IEnumerable<EmpireBorderView> GetEmpireBorderViews(ObjectId empireId);

        void TransferSystems(DeltaMetadata deltaMetadata, Empire sender, Empire receiver, IEnumerable<ObjectId> systemIds);

        void ApplyMilitaryDamage(DeltaMetadata deltaMetadata, ObjectId empireId, double damage);
    }
}