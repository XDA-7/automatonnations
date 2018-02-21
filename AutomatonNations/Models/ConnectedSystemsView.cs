using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class ConnectedSystemsView
    {
        public ObjectId Id { get; set; }

        public IEnumerable<ObjectId> ConnectedSystemIds { get; set; }

        public IEnumerable<StarSystem> ConnectedSystems { get; set; }
    }
}