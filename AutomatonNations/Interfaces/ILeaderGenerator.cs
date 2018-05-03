using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ILeaderGenerator
    {
        IEnumerable<ObjectId> GenerateLeadersForEmpire(DeltaMetadata deltaMetadata, ObjectId empireId);
    }
}