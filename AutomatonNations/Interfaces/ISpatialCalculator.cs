using System.Collections.Generic;

namespace AutomatonNations
{
    public interface ISpatialCalculator
    {
        IEnumerable<Coordinate> WithinRadius(Coordinate centreCoordinate, IEnumerable<Coordinate> coordinates, int radius);
    }
}