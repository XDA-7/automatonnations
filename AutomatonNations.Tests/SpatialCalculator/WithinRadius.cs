using System.Linq;
using Xunit;

namespace AutomatonNations.Tests_SpatialCalculator
{
    public class WithinRadius
    {
        private Coordinate _centreCoordinate = new Coordinate { X = 45, Y = 20 };
        private int _radius = 20;
        private Coordinate[] _inRadius = new Coordinate[]
        {
            new Coordinate { X = 65, Y = 20 },
            new Coordinate { X = 45, Y = 0 },
            new Coordinate { X = 50, Y = 30 },
            new Coordinate { X = 35, Y = 18 },
            new Coordinate { X = 40, Y = 18 }
        };

        private Coordinate[] _outRadius = new Coordinate[]
        {
            new Coordinate { X = 65, Y = 40 },
            new Coordinate { X = 100, Y = 20 },
            new Coordinate { X = 50, Y = 100 },
            new Coordinate { X = 30, Y = 50 },
            new Coordinate { X = 65, Y = 40 }
        };

        private ISpatialCalculator _spatialCalculator;

        public WithinRadius()
        {
            _spatialCalculator = new SpatialCalculator();
        }

        [Fact]
        public void ReturnsCoordinatesInRadiusOfCentre()
        {
            var coordinates = new Coordinate[10];
            _inRadius.CopyTo(coordinates, 0);
            _outRadius.CopyTo(coordinates, 5);

            var result = _spatialCalculator.WithinRadius(_centreCoordinate, coordinates, _radius);

            Assert.Equal(5, result.Count());
            for (var i = 0; i < 5; i++)
            {
                Assert.Contains(result, coord => coord.X == _inRadius[0].X && coord.Y == _inRadius[0].Y);
            }
        }
    }
}