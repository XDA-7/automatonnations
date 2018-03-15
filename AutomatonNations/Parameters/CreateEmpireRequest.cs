using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class CreateEmpireRequest
    {
        public CreateEmpireRequest(Alignment alignment, IEnumerable<ObjectId> starSystemIds)
        {
            Alignment = alignment;
            StarSystemIds = starSystemIds;
        }

        public Alignment Alignment { get; set; }

        public IEnumerable<ObjectId> StarSystemIds { get; set; }
    }
}