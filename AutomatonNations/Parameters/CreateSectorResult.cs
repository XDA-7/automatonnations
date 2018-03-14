using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class CreateSectorResult
    {
        public ObjectId SectorId { get; set; }

        public IEnumerable<StarSystem> StarSystems { get; set; }
    }
}