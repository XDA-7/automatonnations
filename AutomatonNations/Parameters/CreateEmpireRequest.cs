using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class CreateEmpireRequest
    {
        public Alignment Alignment { get; set; }

        public IEnumerable<ObjectId> StarSystemIds { get; set; }
    }
}