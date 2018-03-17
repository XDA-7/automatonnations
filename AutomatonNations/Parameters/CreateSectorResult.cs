using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class CreateSectorResult
    {
        public CreateSectorResult(ObjectId sectorId, IEnumerable<StarSystem> starSystems)
        {
            SectorId = sectorId;
            StarSystems = starSystems;
        }

        public ObjectId SectorId { get; }

        public IEnumerable<StarSystem> StarSystems { get; }
    }
}