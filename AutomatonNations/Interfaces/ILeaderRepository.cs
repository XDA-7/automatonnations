using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ILeaderRepository
    {
        void SetLeadersForEmpire(ObjectId empireId, IEnumerable<Leader> leaders);
    }
}