using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class DeltaRepository : IDeltaRepository
    {
        private IMongoCollection<Delta<double>> _deltaDoubleCollection;

        public DeltaRepository(IDatabaseProvider databaseProvider)
        {
            _deltaDoubleCollection = databaseProvider.Database.GetCollection<Delta<double>>(Collections.Deltas);
        }

        public DeltaSet GetForSimulation(ObjectId simulationId, int startTick, int endTick)
        {
            var deltaDoubles = _deltaDoubleCollection.Find(
                GetDeltaDoublessFilter(simulationId, startTick, endTick)
            ).ToEnumerable();
            return new DeltaSet
            {
                DeltaDoubles = deltaDoubles
            };
        }

        private FilterDefinition<Delta<double>> GetDeltaDoublessFilter(ObjectId simulationId, int startTick, int endTick) =>
            Builders<Delta<double>>.Filter.Eq(delta => delta.SimulationId, simulationId) &
            Builders<Delta<double>>.Filter.Gte(delta => delta.Tick, startTick) &
            Builders<Delta<double>>.Filter.Lte(delta => delta.Tick, endTick);
    }
}