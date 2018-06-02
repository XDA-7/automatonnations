using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class LeaderUpdate
    {
        public IEnumerable<EmpireLeaderUpdate> EmpireUpdates { get; set; }
    }

    public class EmpireLeaderUpdate
    {
        public ObjectId EmpireId { get; set; }
        public IEnumerable<Leader> Leaders { get; set; }
    }
}