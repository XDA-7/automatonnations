using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ISectorRepository
    {
        CreateSectorResult Create(IEnumerable<CreateSectorRequest> requests);

        void ConnectSystems(IEnumerable<StarSystem> starSystems);
    }
}