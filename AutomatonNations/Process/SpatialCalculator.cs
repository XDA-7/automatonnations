using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class SpatialCalculator : ISpatialCalculator
    {
        public IEnumerable<Coordinate> WithinRadius(Coordinate centreCoordinate, IEnumerable<Coordinate> coordinates, int radius)
        {
            var squareRadius = radius * radius;
            return coordinates.Where(x => IsWithinRadius(centreCoordinate, x, squareRadius));
        }

        private bool IsWithinRadius(Coordinate centreCoordinate, Coordinate coordinate, int squareRadius)
        {
            var xDifference = centreCoordinate.X - coordinate.X;
            var yDifference = centreCoordinate.Y - coordinate.Y;
            var squareDistance = (xDifference * xDifference) + (yDifference * yDifference);
            return squareDistance <= squareRadius;
        }
    }
}