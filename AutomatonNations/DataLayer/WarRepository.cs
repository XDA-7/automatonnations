using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class WarRepository : IWarRepository
    {
        private IMongoCollection<War> _warCollection;
        private IMongoCollection<Simulation> _simulationCollection;
        private IMongoCollection<Delta> _deltaCollection;
        private IMongoCollection<Delta<double>> _deltaDoubleCollection;

        public WarRepository(IDatabaseProvider databaseProvider)
        {
            _warCollection = databaseProvider.Database.GetCollection<War>(Collections.Wars);
            _simulationCollection = databaseProvider.Database.GetCollection<Simulation>(Collections.Simulations);
            _deltaCollection = databaseProvider.Database.GetCollection<Delta>(Collections.Deltas);
            _deltaDoubleCollection = databaseProvider.Database.GetCollection<Delta<double>>(Collections.Deltas);
        }

        public IEnumerable<War> GetWars(ObjectId simulationId)
        {
            var simulation = _simulationCollection.Find(GetSimulationById(simulationId)).Single();
            return _warCollection.Find(GetActiveWarsByIds(simulation.WarIds)).ToList();
        }

        public IEnumerable<War> GetWarsForEmpire(ObjectId empireId)
        {
            return _warCollection.Find(GetActiveWarWithParticipant(empireId)).ToList();
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

        public void ContinueWar(DeltaMetadata deltaMetadata, ObjectId warId, double attackerDamage, double defenderDamage)
        {
            _warCollection.UpdateOne(GetWarById(warId), TakeTurn(attackerDamage, defenderDamage));
            _deltaDoubleCollection.InsertMany(new Delta<double>[]
            {
                new Delta<double>
                {
                    DeltaType = DeltaType.WarAttackerDamage,
                    SimulationId = deltaMetadata.SimulationId,
                    Tick = deltaMetadata.Tick,
                    ReferenceId = warId,
                    Value = attackerDamage
                },
                new Delta<double>
                {
                    DeltaType = DeltaType.WarDefenderDamage,
                    SimulationId = deltaMetadata.SimulationId,
                    Tick = deltaMetadata.Tick,
                    ReferenceId = warId,
                    Value = defenderDamage
                }
            });
        }

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

        public void EndWarsWithParticipant(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var wars = _warCollection.Find(GetActiveWarWithParticipant(empireId)).ToList();
            if (!wars.Any())
            {
                return;
            }
            
            _warCollection.UpdateMany(GetActiveWarWithParticipant(empireId), DeactivateWar());
            _deltaCollection.InsertMany(wars.Select(war => new Delta
            {
                DeltaType = DeltaType.WarEnd,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = war.Id
            }));
        }

        private FilterDefinition<Simulation> GetSimulationById(ObjectId id) =>
            Builders<Simulation>.Filter.Eq(simulation => simulation.Id, id);

        private FilterDefinition<War> GetWarById(ObjectId id) =>
            Builders<War>.Filter.Eq(war => war.Id, id);
        
        private FilterDefinition<War> GetActiveWarWithParticipant(ObjectId id) =>
            Builders<War>.Filter.Eq(war => war.Active, true) &
            (Builders<War>.Filter.Eq(war => war.AttackerId, id) |
            Builders<War>.Filter.Eq(war => war.DefenderId, id));

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