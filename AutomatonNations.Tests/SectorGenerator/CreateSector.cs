using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_SectorGenerator
{
    public class CreateSector
    {
        private const int _size = 100;
        private Mock<IRandom> _random = new Mock<IRandom>();
        private Mock<ISectorRepository> _sectorRepository = new Mock<ISectorRepository>();
        private Mock<ISpatialOperations> _spatialOperations = new Mock<ISpatialOperations>();
        private ISectorGenerator _sectorGenerator;

        public CreateSector()
        {
            _sectorGenerator = new SectorGenerator(_random.Object, _sectorRepository.Object, _spatialOperations.Object);        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(98)]
        public void CreateSectorWithSystemsEqualToStarCount(int starCount)
        {
            var mockCoords = new List<int>();
            for (var i = 0; i < starCount; i++)
            {
                mockCoords.Add(i);
                mockCoords.Add(i);
            }

            _random
                .Setup(x => x.NextSet(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(mockCoords.ToArray());

            _sectorGenerator.CreateSector(starCount, _size, It.IsAny<int>());
            _sectorRepository
                .Verify(x => x.Create(It.Is<Coordinate[]>(y => y.Length == starCount)), Times.Once);
        }

        [Theory]
        [InlineData(new int[] { 50, 32, 12, 93, 35, 61 })]
        [InlineData(new int[] { 67, 3, 42, 69, 88, 94 })]
        public void SavesSystemsWithValuesReturnedByRandom(int[] nextSet)
        {
            _random
                .Setup(x => x.NextSet(_size, 6))
                .Returns(nextSet);
            
            _sectorGenerator.CreateSector(3, _size, It.IsAny<int>());
            _sectorRepository
                .Verify(x => x.Create(It.Is<Coordinate[]>(
                    coord =>
                    coord[0].X == nextSet[0] &&
                    coord[0].Y == nextSet[1] &&
                    coord[1].X == nextSet[2] &&
                    coord[1].Y == nextSet[3] &&
                    coord[2].X == nextSet[4] &&
                    coord[2].Y == nextSet[5]
                )), Times.Once);
        }

        [Fact]
        public void UsesCoordinatesInRangeOfSize()
        {
            _sectorGenerator.CreateSector(It.IsAny<int>(), _size, It.IsAny<int>());

            _random
                .Verify(x => x.NextSet(_size, It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData(0, 0, 0, 3, 0, 0)]
        [InlineData(13, 13, 34, 13, 0, 0)]
        [InlineData(22, 9, 22, 9, 13, 12)]
        public void DoesNotCreateSystemsWithDuplicateCoordinates(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            _random
                .Setup(x => x.NextSet(_size, 4))
                .Returns(new int[] { x1, y1, x1, y1 });
            var replacementRequest = 0;
            _random
                .Setup(x => x.NextSet(_size, 2))
                .Returns(() =>
                {
                    if (replacementRequest == 0)
                    {
                        replacementRequest++;
                        return new int[] { x2, y2 };
                    }
                    else
                    {
                        return new int[] { x3, y3 };
                    }
                });

            _sectorGenerator.CreateSector(2, _size, It.IsAny<int>());
            _sectorRepository
                .Verify(x => x.Create(It.Is<Coordinate[]>(
                    y =>
                    y[0].X != y[1].X ||
                    y[0].Y != y[1].Y)), Times.Once);
        }

        [Fact]
        public void ConnectsSystemsToThoseWithinRadius()
        {
            var one = "000000000000000000000001";
            var two = "000000000000000000000002";
            var three = "000000000000000000000003";
            var four = "000000000000000000000004";
            var five = "000000000000000000000005";
            _sectorRepository
                .Setup(x => x.Create(It.IsAny<Coordinate[]>()))
                .Returns(new StarSystem[]
                {
                    new StarSystem { Id = new ObjectId(one), Coordinate = new Coordinate { X = 1, Y = 1 } },
                    new StarSystem { Id = new ObjectId(two), Coordinate = new Coordinate { X = 2, Y = 2 } },
                    new StarSystem { Id = new ObjectId(three), Coordinate = new Coordinate { X = 3, Y = 3 } },
                    new StarSystem { Id = new ObjectId(four), Coordinate = new Coordinate { X = 4, Y = 4 } },
                    new StarSystem { Id = new ObjectId(five), Coordinate = new Coordinate { X = 5, Y = 5 } }
                });

            _spatialOperations
                .Setup(x => x.WithinRadius(
                    It.Is<Coordinate>(coord => coord.X == 1 && coord.Y == 1),
                    It.IsAny<Coordinate[]>(),
                    It.IsAny<int>()))
                .Returns(new Coordinate[]
                {
                    new Coordinate { X = 3, Y = 3 },
                    new Coordinate { X = 4, Y = 4 }
                });
            _spatialOperations
                .Setup(x => x.WithinRadius(
                    It.Is<Coordinate>(coord => coord.X == 4 && coord.Y == 4),
                    It.IsAny<Coordinate[]>(),
                    It.IsAny<int>()))
                .Returns(new Coordinate[]
                {
                    new Coordinate { X = 2, Y = 2 },
                    new Coordinate { X = 3, Y = 3 }
                });

            _sectorGenerator.CreateSector(It.IsAny<int>(), _size, It.IsAny<int>());

            _sectorRepository
                .Verify(x => x.ConnectSystems(It.Is<StarSystem[]>(
                    y =>
                    y[0].ConnectedSystems.Contains(new ObjectId(three)) &&
                    y[0].ConnectedSystems.Contains(new ObjectId(four)) &&
                    !y[1].ConnectedSystems.Any() &&
                    !y[2].ConnectedSystems.Any() &&
                    y[3].ConnectedSystems.Contains(new ObjectId(two)) &&
                    y[3].ConnectedSystems.Contains(new ObjectId(three)) &&
                    !y[4].ConnectedSystems.Any()
                )), Times.Once);
        }
    }
}