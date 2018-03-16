using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class DeltaRepository : IDeltaRepository
    {
        private IMongoCollection<Delta<decimal>> _deltaDecimalCollection;

        public DeltaRepository(IDatabaseProvider databaseProvider)
        {
            _deltaDecimalCollection = databaseProvider.Database.GetCollection<Delta<decimal>>(Collections.Deltas);
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
            Builders<Delta<decimal>>.Filter.Eq(delta => delta.SimulationId, simulationId) &
            Builders<Delta<decimal>>.Filter.Gte(delta => delta.Tick, startTick) &
            Builders<Delta<decimal>>.Filter.Lte(delta => delta.Tick, endTick);
    }
}