using System.Collections.Generic;

namespace AutomatonNations
{
    public interface ISpatialOperations
    {
        IEnumerable<Coordinate> WithinRadius(Coordinate centreCoordinate, IEnumerable<Coordinate> coordinates, int radius);
    }
}