using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class DeltaRepository : IDeltaRepository
    {
        private IMongoCollection<Delta<decimal>> _deltaDecimalCollection;
        private FilterDefinitionBuilder<Delta<decimal>> _deltaDecimalFilterBuilder;

        public DeltaRepository(IDatabaseProvider databaseProvider)
        {
            _deltaDecimalCollection = databaseProvider.Database.GetCollection<Delta<decimal>>(Collections.Deltas);
            _deltaDecimalFilterBuilder = Builders<Delta<decimal>>.Filter;
        }

        public DeltaSet GetForSimulation(ObjectId simulationId, int startTick, int endTick)
        {
            var deltaDecimals = _deltaDecimalCollection.Find(
                GetDeltaDecimalsFilter(simulationId, startTick, endTick)
            ).ToEnumerable();
            return new DeltaSet
            {
                DeltaDecimals = deltaDecimals
            };
        }

        private FilterDefinition<Delta<decimal>> GetDeltaDecimalsFilter(ObjectId simulationId, int startTick, int endTick) =>
            _deltaDecimalFilterBuilder.Eq(delta => delta.SimulationId, simulationId) &
            _deltaDecimalFilterBuilder.Gte(delta => delta.Tick, startTick) &
            _deltaDecimalFilterBuilder.Lte(delta => delta.Tick, endTick);
    }
}