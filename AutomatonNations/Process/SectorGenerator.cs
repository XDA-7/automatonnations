using System.Collections.Generic;
using System.Linq;

namespace AutomatonNations
{
    public class SectorGenerator : ISectorGenerator
    {
        private IRandom _random;
        private ISectorRepository _sectorRepository;
        private ISpatialOperations _spatialOperations;

        public SectorGenerator(IRandom random, ISectorRepository sectorRepository, ISpatialOperations spatialOperations)
        {
            _random = random;
            _sectorRepository = sectorRepository;
            _spatialOperations = spatialOperations;
        }

        public void CreateSector(int starCount, int size, int connectivityRadius)
        {
            var coordinates = GetCoordinates(starCount, size);
            var starSystems = _sectorRepository.Create(coordinates);
            ConnectSystems(starSystems, coordinates, connectivityRadius);
            _sectorRepository.ConnectSystems(starSystems);
        }

        private void ConnectSystems(IEnumerable<StarSystem> starSystems, IEnumerable<Coordinate> coordinates, int connectivityRadius)
        {
            foreach (var starSystem in starSystems)
            {
                var inRadiusCoordinates = _spatialOperations.WithinRadius(starSystem.Coordinate, coordinates, connectivityRadius);
                starSystem.ConnectedSystems = starSystems
                    .Where(system => system != starSystem)
                    .Where(system => IsSystemInRadius(system, inRadiusCoordinates))
                    .Select(x => x.Id);
            }
        }

        private bool IsSystemInRadius(StarSystem system, IEnumerable<Coordinate> inRadiusCoordinates) =>
            inRadiusCoordinates.Any(coord => system.Coordinate.X == coord.X && system.Coordinate.Y == coord.Y);

        private IEnumerable<Coordinate> GetCoordinates(int count, int mapSize)
        {
            var coordinatesOccupied = new bool[mapSize, mapSize];
            var result = new Coordinate[count];
            var coords = _random.NextSet(mapSize, count * 2);
            for (var i = 0; i < count; i++)
            {
                var x = coords[i * 2];
                var y = coords[i * 2 + 1];
                result[i] = GetCoordinate(x, y, mapSize, coordinatesOccupied);
            }

            return result;
        }

        private Coordinate GetCoordinate(int x, int y, int mapSize, bool[,] coordinatesOccupied)
        {
            if (coordinatesOccupied[x, y])
            {
                var newCoords = _random.NextSet(mapSize, 2);
                return GetCoordinate(newCoords[0], newCoords[1], mapSize, coordinatesOccupied);
            }
            else
            {
                coordinatesOccupied[x, y] = true;
                return new Coordinate { X = x, Y = y };
            }
        }
    }
}