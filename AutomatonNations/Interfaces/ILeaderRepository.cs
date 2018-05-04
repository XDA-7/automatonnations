using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ILeaderRepository
    {
        void SetLeadersForEmpire(DeltaMetadata deltaMetadata, ObjectId empireId, IEnumerable<Leader> leaders);
    }
}