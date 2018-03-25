using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class WarRepository : IWarRepository
    {
        private IMongoCollection<War> _warCollection;
        private IMongoCollection<Simulation> _simulationCollection;
        private IMongoCollection<Delta> _deltaCollection;

        public WarRepository(IDatabaseProvider databaseProvider)
        {
            _warCollection = databaseProvider.Database.GetCollection<War>(Collections.Wars);
            _simulationCollection = databaseProvider.Database.GetCollection<Simulation>(Collections.Simulations);
            _deltaCollection = databaseProvider.Database.GetCollection<Delta>(Collections.Deltas);
        }

        public IEnumerable<War> GetWars(ObjectId simulationId)
        {
            var simulation = _simulationCollection.Find(GetSimulationById(simulationId)).Single();
            return _warCollection.Find(GetActiveWarsByIds(simulation.WarIds)).ToEnumerable();
        }

        public IEnumerable<War> GetWarsForEmpire(ObjectId empireId)
        {
            return _warCollection.Find(GetActiveWarWithParticipant(empireId)).ToEnumerable();
        }

        public ObjectId BeginWar(DeltaMetadata deltaMetadata, ObjectId attackerId, ObjectId defenderId)
        {
            var war = new War
            {
                AttackerId = attackerId,
                DefenderId = defenderId,
                Active = true
            };
            _warCollection.InsertOne(war);
            _simulationCollection.UpdateOne(GetSimulationById(deltaMetadata.SimulationId), AddWarToSimulation(war.Id));
            _deltaCollection.InsertOne(new Delta
            {
                DeltaType = DeltaType.WarBegin,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = war.Id
            });

            return war.Id;
        }

        public void ContinueWar(ObjectId warId, double attackerDamage, double defenderDamage) =>
            _warCollection.UpdateOne(GetWarById(warId), TakeTurn(attackerDamage, defenderDamage));

        public void EndWar(DeltaMetadata deltaMetadata, ObjectId warId)
        {
            _warCollection.UpdateOne(GetWarById(warId), DeactivateWar());
            _deltaCollection.InsertOne(new Delta
            {
                DeltaType = DeltaType.WarEnd,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = warId
            });
        }

        private FilterDefinition<Simulation> GetSimulationById(ObjectId id) =>
            Builders<Simulation>.Filter.Eq(simulation => simulation.Id, id);

        private FilterDefinition<War> GetWarById(ObjectId id) =>
            Builders<War>.Filter.Eq(war => war.Id, id);
        
        private FilterDefinition<War> GetActiveWarWithParticipant(ObjectId id) =>
            Builders<War>.Filter.Eq(war => war.Active, true) &
            Builders<War>.Filter.Eq(war => war.AttackerId, id) &
            Builders<War>.Filter.Eq(war => war.DefenderId, id);

        private FilterDefinition<War> GetActiveWarsByIds(IEnumerable<ObjectId> ids) =>
            Builders<War>.Filter.In(war => war.Id, ids) &
                Builders<War>.Filter.Eq(war => war.Active, true);
        
        private UpdateDefinition<Simulation> AddWarToSimulation(ObjectId id) =>
            Builders<Simulation>.Update.Push(simulation => simulation.WarIds, id);
        
        private UpdateDefinition<War> TakeTurn(double attackerDamage, double defenderDamage) =>
            Builders<War>.Update.Inc(war => war.AttackerDamage, attackerDamage)
                .Inc(war => war.DefenderDamage, defenderDamage)
                .Inc(war => war.Ticks, 1);
        
        private UpdateDefinition<War> DeactivateWar() =>
            Builders<War>.Update.Set(war => war.Active, false);
    }
}