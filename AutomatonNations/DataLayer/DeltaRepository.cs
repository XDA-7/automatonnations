using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class DeltaRepository : IDeltaRepository
    {
        private IMongoCollection<Delta<object>> _deltaCollection;
        private IMongoCollection<Delta<double>> _deltaDoubleCollection;
        private IMongoCollection<Delta<ObjectId>> _deltaObjectIdCollection;

        public DeltaRepository(IDatabaseProvider databaseProvider)
        {
            _deltaCollection = databaseProvider.Database.GetCollection<Delta<object>>(Collections.Deltas);
            _deltaDoubleCollection = databaseProvider.Database.GetCollection<Delta<double>>(Collections.Deltas);
            _deltaObjectIdCollection = databaseProvider.Database.GetCollection<Delta<ObjectId>>(Collections.Deltas);
        }

        public DeltaSet GetForSimulation(ObjectId simulationId, int startTick, int endTick)
        {
            var deltas = _deltaCollection.Find(
                GetDeltaFilter(simulationId, startTick, endTick)
            )
            .ToEnumerable()
            .Select(x => new Delta
            {
                Id = x.Id,
                DeltaType = x.DeltaType,
                Tick = x.Tick,
                ReferenceId = x.ReferenceId,
                SimulationId = x.SimulationId
            });
            var deltaDoubles = _deltaDoubleCollection.Find(
                GetDeltaFilter<double>(simulationId, BsonType.Double, startTick, endTick)
            ).ToEnumerable();
            var deltaObjectIds = _deltaObjectIdCollection.Find(
                GetDeltaFilter<ObjectId>(simulationId, BsonType.ObjectId, startTick, endTick)
            ).ToEnumerable();
            return new DeltaSet
            {
                Deltas = deltas,
                DeltaDoubles = deltaDoubles,
                DeltaObjectIds = deltaObjectIds
            };
        }

        private FilterDefinition<Delta<object>> GetDeltaFilter(ObjectId simulationId, int startTick, int endTick) =>
            !Builders<Delta<object>>.Filter.Exists(delta => delta.Value) &
            Builders<Delta<object>>.Filter.Eq(delta => delta.SimulationId, simulationId) &
            Builders<Delta<object>>.Filter.Gte(delta => delta.Tick, startTick) &
            Builders<Delta<object>>.Filter.Lte(delta => delta.Tick, endTick);

        private FilterDefinition<Delta<T>> GetDeltaFilter<T>(ObjectId simulationId, BsonType bsonType, int startTick, int endTick) =>
            Builders<Delta<T>>.Filter.Type(delta => delta.Value, bsonType) &
            Builders<Delta<T>>.Filter.Eq(delta => delta.SimulationId, simulationId) &
            Builders<Delta<T>>.Filter.Gte(delta => delta.Tick, startTick) &
            Builders<Delta<T>>.Filter.Lte(delta => delta.Tick, endTick);
    }
}