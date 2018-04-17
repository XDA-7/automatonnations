using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class DeltaSet
    {
        public IEnumerable<Delta> Deltas { get; set; }

        public IEnumerable<Delta<double>> DeltaDoubles { get; set; }

        public IEnumerable<Delta<ObjectId>> DeltaObjectIds { get; set; }
    }
}