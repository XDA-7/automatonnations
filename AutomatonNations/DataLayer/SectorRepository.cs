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

        public SectorRepository(IDatabaseProvider databaseProvider)
        {
            _sectorCollection = databaseProvider.Database.GetCollection<Sector>(Collections.Sectors);
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
        }

        public CreateSectorResult Create(IEnumerable<CreateSystemRequest> requests)
        {
            var systems = requests.Select(request => new StarSystem
            {
                Coordinate = request.Coordinate,
                Development = request.Development
            }).ToArray();
            if (systems.Any())
            {
                _starSystemCollection.InsertMany(systems);
            }
            
            var sector = new Sector { StarSystemIds = systems.Select(x => x.Id) };
            _sectorCollection.InsertOne(sector);
            return new CreateSectorResult(sector.Id, systems);
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
            Builders<StarSystem>.Filter.Eq(system => system.Id, id);

        private UpdateDefinition<StarSystem> UpdateConnectedSystems(IEnumerable<ObjectId> objectIds) =>
            Builders<StarSystem>.Update.Set(system => system.ConnectedSystemIds, objectIds);
    }
}
