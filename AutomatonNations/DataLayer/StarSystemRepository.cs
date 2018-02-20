using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class StarSystemRepository : IStarSystemRepository
    {
        private IMongoCollection<StarSystem> _starSystemCollection;
        private IMongoCollection<DecimalDelta> _decimalDeltaCollection;
        private FilterDefinitionBuilder<StarSystem> _starSystemFilterBuilder;
        private UpdateDefinitionBuilder<StarSystem> _starSystemUpdateBuilder;

        public StarSystemRepository(IDatabaseProvider databaseProvider)
        {
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _decimalDeltaCollection = databaseProvider.Database.GetCollection<DecimalDelta>(Collections.Deltas);
            _starSystemFilterBuilder = Builders<StarSystem>.Filter;
            _starSystemUpdateBuilder = Builders<StarSystem>.Update;
        }

        public void ApplyDevelopment(IEnumerable<DecimalDelta> deltas)
        {
            _decimalDeltaCollection.InsertMany(deltas);
            foreach (var delta in deltas)
            {
                var filter = GetById(delta.Id);
                var update = GetUpdateDevelopment(delta.Value);
                _starSystemCollection.UpdateOne(filter, update);
            }
        }

        private FilterDefinition<StarSystem> GetById(ObjectId id) =>
            _starSystemFilterBuilder.Eq(system => system.Id, id);

        private UpdateDefinition<StarSystem> GetUpdateDevelopment(decimal delta) =>
            _starSystemUpdateBuilder.Inc(system => system.Development, delta);
    }
}