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
        private Mock<ISpatialCalculator> _spatialCalculator = new Mock<ISpatialCalculator>();
        private ISectorGenerator _sectorGenerator;

        public CreateSector()
        {
            _sectorGenerator = new SectorGenerator(_random.Object, _sectorRepository.Object, _spatialCalculator.Object);        }

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
                .Setup(x => x.IntegerSet(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(mockCoords.ToArray());
            _sectorRepository.Setup(x => x.Create(It.IsAny<IEnumerable<CreateSystemRequest>>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, new StarSystem[0]));


            _sectorGenerator.CreateSector(starCount, _size, It.IsAny<int>(), It.IsAny<int>());
            _sectorRepository
                .Verify(x => x.Create(It.Is<IEnumerable<CreateSystemRequest>>(y => y.Count() == starCount)), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-230)]
        [InlineData(68)]
        public void CreateSystemsWithBaseDevelopment(int baseDevelopment)
        {
            _random
                .Setup(x => x.IntegerSet(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new int[] { 0, 0, 1, 1 });
            _sectorRepository.Setup(x => x.Create(It.IsAny<IEnumerable<CreateSystemRequest>>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, new StarSystem[0]));
            
            _sectorGenerator.CreateSector(2, _size, It.IsAny<int>(), baseDevelopment);

            _sectorRepository.Verify(
                x => x.Create(
                    It.Is<IEnumerable<CreateSystemRequest>>(
                        requests => requests.All(
                            request => request.Development == baseDevelopment))),
                Times.Once);
        }

        [Theory]
        [InlineData(new int[] { 50, 32, 12, 93, 35, 61 })]
        [InlineData(new int[] { 67, 3, 42, 69, 88, 94 })]
        public void SavesSystemsWithValuesReturnedByRandom(int[] nextSet)
        {
            _random
                .Setup(x => x.IntegerSet(_size, 6))
                .Returns(nextSet);
            _sectorRepository.Setup(x => x.Create(It.IsAny<IEnumerable<CreateSystemRequest>>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, new StarSystem[0]));
            
            _sectorGenerator.CreateSector(3, _size, It.IsAny<int>(), It.IsAny<int>());
            _sectorRepository
                .Verify(x => x.Create(It.Is<IEnumerable<CreateSystemRequest>>(
                    request =>
                    request.ToArray()[0].Coordinate.X == nextSet[0] &&
                    request.ToArray()[0].Coordinate.Y == nextSet[1] &&
                    request.ToArray()[1].Coordinate.X == nextSet[2] &&
                    request.ToArray()[1].Coordinate.Y == nextSet[3] &&
                    request.ToArray()[2].Coordinate.X == nextSet[4] &&
                    request.ToArray()[2].Coordinate.Y == nextSet[5]
                )), Times.Once);
        }

        [Fact]
        public void UsesCoordinatesInRangeOfSize()
        {
            _sectorRepository.Setup(x => x.Create(It.IsAny<IEnumerable<CreateSystemRequest>>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, new StarSystem[0]));
                
            _sectorGenerator.CreateSector(It.IsAny<int>(), _size, It.IsAny<int>(), It.IsAny<int>());

            _random
                .Verify(x => x.IntegerSet(_size, It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData(0, 0, 0, 3, 0, 0)]
        [InlineData(13, 13, 34, 13, 0, 0)]
        [InlineData(22, 9, 22, 9, 13, 12)]
        public void DoesNotCreateSystemsWithDuplicateCoordinates(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            _random
                .Setup(x => x.IntegerSet(_size, 4))
                .Returns(new int[] { x1, y1, x1, y1 });
            var replacementRequest = 0;
            _random
                .Setup(x => x.IntegerSet(_size, 2))
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
            _sectorRepository.Setup(x => x.Create(It.IsAny<IEnumerable<CreateSystemRequest>>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, new StarSystem[0]));

            _sectorGenerator.CreateSector(2, _size, It.IsAny<int>(), It.IsAny<int>());
            _sectorRepository
                .Verify(x => x.Create(It.Is<IEnumerable<CreateSystemRequest>>(
                    request =>
                    request.ToArray()[0].Coordinate.X != request.ToArray()[1].Coordinate.X ||
                    request.ToArray()[0].Coordinate.Y != request.ToArray()[1].Coordinate.Y)), Times.Once);
        }

        [Fact]
        public void ConnectsSystemsToThoseWithinRadius()
        {
            var one = "000000000000000000000001";
            var two = "000000000000000000000002";
            var three = "000000000000000000000003";
            var four = "000000000000000000000004";
            var five = "000000000000000000000005";
            var systems = new StarSystem[]
            {
                new StarSystem { Id = new ObjectId(one), Coordinate = new Coordinate { X = 1, Y = 1 } },
                new StarSystem { Id = new ObjectId(two), Coordinate = new Coordinate { X = 2, Y = 2 } },
                new StarSystem { Id = new ObjectId(three), Coordinate = new Coordinate { X = 3, Y = 3 } },
                new StarSystem { Id = new ObjectId(four), Coordinate = new Coordinate { X = 4, Y = 4 } },
                new StarSystem { Id = new ObjectId(five), Coordinate = new Coordinate { X = 5, Y = 5 } }
            };
            _sectorRepository
                .Setup(x => x.Create(It.IsAny<IEnumerable<CreateSystemRequest>>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, systems));

            _spatialCalculator
                .Setup(x => x.WithinRadius(
                    It.Is<Coordinate>(coord => coord.X == 1 && coord.Y == 1),
                    It.IsAny<IEnumerable<Coordinate>>(),
                    It.IsAny<int>()))
                .Returns(new Coordinate[]
                {
                    new Coordinate { X = 3, Y = 3 },
                    new Coordinate { X = 4, Y = 4 }
                });
            _spatialCalculator
                .Setup(x => x.WithinRadius(
                    It.Is<Coordinate>(coord => coord.X == 4 && coord.Y == 4),
                    It.IsAny<IEnumerable<Coordinate>>(),
                    It.IsAny<int>()))
                .Returns(new Coordinate[]
                {
                    new Coordinate { X = 2, Y = 2 },
                    new Coordinate { X = 3, Y = 3 }
                });

            _sectorGenerator.CreateSector(It.IsAny<int>(), _size, It.IsAny<int>(), It.IsAny<int>());

            _sectorRepository
                .Verify(x => x.ConnectSystems(It.Is<IEnumerable<StarSystem>>(
                    y =>
                    y.ToArray()[0].ConnectedSystemIds.Contains(new ObjectId(three)) &&
                    y.ToArray()[0].ConnectedSystemIds.Contains(new ObjectId(four)) &&
                    !y.ToArray()[1].ConnectedSystemIds.Any() &&
                    !y.ToArray()[2].ConnectedSystemIds.Any() &&
                    y.ToArray()[3].ConnectedSystemIds.Contains(new ObjectId(two)) &&
                    y.ToArray()[3].ConnectedSystemIds.Contains(new ObjectId(three)) &&
                    !y.ToArray()[4].ConnectedSystemIds.Any()
                )), Times.Once);
        }

        [Fact]
        public void DoesNotConnectSystemToItself()
        {
            var systems = new StarSystem[]
            {
                new StarSystem { Coordinate = new Coordinate { X = 45, Y = 18 } }
            };
            _sectorRepository
                .Setup(x => x.Create(It.IsAny<IEnumerable<CreateSystemRequest>>()))
                .Returns(new CreateSectorResult(ObjectId.Empty, systems));
            _spatialCalculator
                .Setup(x => x.WithinRadius(It.IsAny<Coordinate>(), It.IsAny<IEnumerable<Coordinate>>(), It.IsAny<int>()))
                .Returns(new Coordinate[] { new Coordinate { X = 45, Y = 18 } });
            
            _sectorGenerator.CreateSector(It.IsAny<int>(), _size, It.IsAny<int>(), It.IsAny<int>());

            _sectorRepository
                .Verify(x => x.ConnectSystems(It.Is<IEnumerable<StarSystem>>(y => y.ToArray()[0].ConnectedSystemIds.Count() == 0)), Times.Once);
        }
    }
}