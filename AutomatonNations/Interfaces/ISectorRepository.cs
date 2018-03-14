using System.Collections.Generic;
using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ISectorRepository
    {
        CreateSectorResult Create(IEnumerable<Coordinate> coordinates);

        void ConnectSystems(IEnumerable<StarSystem> starSystems);
    }
}