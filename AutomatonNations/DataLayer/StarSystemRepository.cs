using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class StarSystemRepository : IStarSystemRepository
    {
        private IMongoCollection<StarSystem> _starSystemCollection;
        private IMongoCollection<Delta<decimal>> _decimalDeltaCollection;

        public StarSystemRepository(IDatabaseProvider databaseProvider)
        {
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _decimalDeltaCollection = databaseProvider.Database.GetCollection<Delta<decimal>>(Collections.Deltas);
        }

        public void ApplyDevelopment(IEnumerable<Delta<decimal>> deltas)
        {
            _decimalDeltaCollection.InsertMany(deltas);
            foreach (var delta in deltas)
            {
                var filter = GetById(delta.ReferenceId);
                var update = GetUpdateDevelopment(delta.Value);
                _starSystemCollection.UpdateOne(filter, update);
            }
        }

        public ConnectedSystemsView GetConnectedSystems(ObjectId systemId)
        {
            var getSystem = GetById(systemId);
            var starSystem = _starSystemCollection.Find(getSystem).Single();
            var getConnected = GetInIds(starSystem.ConnectedSystemIds);
            var connectedSystems = _starSystemCollection.Find(getConnected).ToEnumerable();
            return new ConnectedSystemsView
            {
                StarSystem = starSystem,
                ConnectedSystems = connectedSystems
            };
        }

        private FilterDefinition<StarSystem> GetById(ObjectId id) =>
            Builders<StarSystem>.Filter.Eq(system => system.Id, id);
        
        private FilterDefinition<StarSystem> GetInIds(IEnumerable<ObjectId> ids) =>
           Builders<StarSystem>.Filter.In(system => system.Id, ids);

        private UpdateDefinition<StarSystem> GetUpdateDevelopment(decimal delta) =>
            Builders<StarSystem>.Update.Inc(system => system.Development, delta);
    }    
}