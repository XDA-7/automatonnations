using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class StarSystemRepository : IStarSystemRepository
    {
        private IMongoCollection<StarSystem> _starSystemCollection;
        private IMongoCollection<Delta<decimal>> _decimalDeltaCollection;
        private FilterDefinitionBuilder<StarSystem> _starSystemFilterBuilder;
        private UpdateDefinitionBuilder<StarSystem> _starSystemUpdateBuilder;

        public StarSystemRepository(IDatabaseProvider databaseProvider)
        {
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _decimalDeltaCollection = databaseProvider.Database.GetCollection<Delta<decimal>>(Collections.Deltas);
            _starSystemFilterBuilder = Builders<StarSystem>.Filter;
            _starSystemUpdateBuilder = Builders<StarSystem>.Update;
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
            _starSystemFilterBuilder.Eq(system => system.Id, id);
        
        private FilterDefinition<StarSystem> GetInIds(IEnumerable<ObjectId> ids) =>
            _starSystemFilterBuilder.In(system => system.Id, ids);

        private UpdateDefinition<StarSystem> GetUpdateDevelopment(decimal delta) =>
            _starSystemUpdateBuilder.Inc(system => system.Development, delta);
    }    
}