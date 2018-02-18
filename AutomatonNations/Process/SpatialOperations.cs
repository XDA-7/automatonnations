using System.Collections.Generic;

namespace AutomatonNations
{
    public class SpatialOperations : ISpatialOperations
    {
        public Coordinate[] WithinRadius(Coordinate centreCoordinate, Coordinate[] coordinates, int radius)
        {
            var squareRadius = radius * radius;
            var result = new List<Coordinate>();
            foreach (var coordinate in coordinates)
            {
                if (IsWithinRadius(centreCoordinate, coordinate, squareRadius))
                {
                    result.Add(coordinate);
                }
            }

            return result.ToArray();
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