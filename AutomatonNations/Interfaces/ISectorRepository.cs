using System.Collections.Generic;

namespace AutomatonNations
{
    public interface ISectorRepository
    {
        IEnumerable<StarSystem> Create(IEnumerable<Coordinate> coordinates);

        void ConnectSystems(IEnumerable<StarSystem> starSystems);
    }
}