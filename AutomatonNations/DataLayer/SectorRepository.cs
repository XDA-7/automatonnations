using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class SectorRepository : ISectorRepository
    {
        private IMongoCollection<Sector> _sectorCollection;
        private IMongoCollection<StarSystem> _starSystemCollection;
        private FilterDefinitionBuilder<StarSystem> _starSystemFilerBuilder;
        private UpdateDefinitionBuilder<StarSystem> _starSystemUpdateBuilder;

        public SectorRepository(IDatabaseProvider databaseProvider)
        {
            _sectorCollection = databaseProvider.Database.GetCollection<Sector>(Collections.Sectors);
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _starSystemFilerBuilder = Builders<StarSystem>.Filter;
            _starSystemUpdateBuilder = Builders<StarSystem>.Update;
        }

        public CreateSectorResult Create(IEnumerable<Coordinate> coordinates)
        {
            var systems = coordinates.Select(x => new StarSystem { Coordinate = x }).ToArray();
            _starSystemCollection.InsertMany(systems);
            var sector = new Sector { StarSystemIds = systems.Select(x => x.Id) };
            _sectorCollection.InsertOne(sector);
            return new CreateSectorResult
            {
                SectorId = sector.Id,
                StarSystems = systems
            };
        }

        public void ConnectSystems(IEnumerable<StarSystem> starSystems)
        {
            foreach (var system in starSystems)
            {
                _starSystemCollection.UpdateOne(
                    GetStarSystemById(system.Id),
                    UpdateConnectedSystems(system.ConnectedSystemIds)
                );
            }
        }

        private FilterDefinition<StarSystem> GetStarSystemById(ObjectId id) =>
            _starSystemFilerBuilder.Eq(system => system.Id, id);

        private UpdateDefinition<StarSystem> UpdateConnectedSystems(IEnumerable<ObjectId> objectIds) =>
            _starSystemUpdateBuilder.Set(system => system.ConnectedSystemIds, objectIds);
    }
}
