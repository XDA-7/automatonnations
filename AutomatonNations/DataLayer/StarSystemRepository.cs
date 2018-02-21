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
        private ProjectionDefinitionBuilder<StarSystem> _starSystemProjectionBuilder;

        public StarSystemRepository(IDatabaseProvider databaseProvider)
        {
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _decimalDeltaCollection = databaseProvider.Database.GetCollection<DecimalDelta>(Collections.Deltas);
            _starSystemFilterBuilder = Builders<StarSystem>.Filter;
            _starSystemUpdateBuilder = Builders<StarSystem>.Update;
            _starSystemProjectionBuilder = Builders<StarSystem>.Projection;
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

        public ConnectedSystemsView GetConnectedSystems(ObjectId systemId)
        {
            var pipeline = GetConnectedSystemsDefinition(systemId);
            var aggregate = _starSystemCollection.Aggregate(pipeline).ToList();
            return aggregate[0];
        }

        private FilterDefinition<StarSystem> GetById(ObjectId id) =>
            _starSystemFilterBuilder.Eq(system => system.Id, id);

        private UpdateDefinition<StarSystem> GetUpdateDevelopment(decimal delta) =>
            _starSystemUpdateBuilder.Inc(system => system.Development, delta);
        
        private PipelineDefinition<StarSystem, ConnectedSystemsView> GetConnectedSystemsDefinition(ObjectId systemId)
        {
            var matchDef = PipelineStageDefinitionBuilder.Match(GetById(systemId));
            var projectionDef = PipelineStageDefinitionBuilder.Project<StarSystem, StarSystem>(
                _starSystemProjectionBuilder.Include(system => system.ConnectedSystemIds)
            );
            var lookupDef = PipelineStageDefinitionBuilder.Lookup<StarSystem, StarSystem, ConnectedSystemsView>(
                _starSystemCollection,
                localSystem => localSystem.ConnectedSystemIds,
                foreignSystem => foreignSystem.Id,
                connectedSystems => connectedSystems.ConnectedSystems
            );
            return new IPipelineStageDefinition[] { matchDef, projectionDef, lookupDef };
        }
    }
}