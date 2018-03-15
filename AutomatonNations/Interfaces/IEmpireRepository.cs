using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface IEmpireRepository
    {
        IEnumerable<ObjectId> Create(IEnumerable<CreateEmpireRequest> requests);

        IEnumerable<EmpireSystemsView> GetEmpireSystemsViews(IEnumerable<ObjectId> empireIds);
    }
}