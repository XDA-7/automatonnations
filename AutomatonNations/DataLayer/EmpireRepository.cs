using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class EmpireRepository : IEmpireRepository
    {
        private IMongoCollection<Empire> _empireCollection;
        private IMongoCollection<StarSystem> _starSystemCollection;
        private IMongoCollection<Delta<ObjectId>> _deltaObjectCollection;
        private IMongoCollection<Delta<double>> _deltaDoubleCollection;
        private IMongoCollection<Delta> _deltaCollection;

        public EmpireRepository(IDatabaseProvider databaseProvider)
        {
            _empireCollection = databaseProvider.Database.GetCollection<Empire>(Collections.Empires);
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _deltaObjectCollection = databaseProvider.Database.GetCollection<Delta<ObjectId>>(Collections.Deltas);
            _deltaDoubleCollection = databaseProvider.Database.GetCollection<Delta<double>>(Collections.Deltas);
            _deltaCollection = databaseProvider.Database.GetCollection<Delta>(Collections.Deltas);
        }

        public IEnumerable<ObjectId> Create(IEnumerable<CreateEmpireRequest> requests)
        {
            if (!requests.Any())
            {
                return new ObjectId[0];
            }

            var empires = requests.Select(x => new Empire
            {
                Alignment = x.Alignment,
                StarSystemsIds = x.StarSystemIds,
                Leaders = new Leader[0]
            }).ToArray();

            _empireCollection.InsertMany(empires);
            return empires.Select(x => x.Id);
        }

        public IEnumerable<ObjectId> Create(DeltaMetadata deltaMetadata, IEnumerable<CreateEmpireRequest> requests)
        {
            var result = Create(requests);
            var deltas = result.Select(id => new Delta
            {
                DeltaType = DeltaType.EmpireCreated,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = id
            });
            _deltaCollection.InsertMany(deltas);
            return result;
        }

        public Empire GetById(ObjectId empireId) =>
            _empireCollection.Find(GetEmpireById(empireId)).Single();
        
        public IEnumerable<Empire> GetByIds(IEnumerable<ObjectId> empireIds) =>
            _empireCollection.Find(GetEmpiresByIds(empireIds)).ToList();

        public EmpireSystemsView GetEmpireSystemsView(ObjectId empireId)
        {
            var empire = _empireCollection.Find(GetEmpireById(empireId)).Single();
            var starSystems = _starSystemCollection.Find(GetStarSystemsByIds(empire.StarSystemsIds)).ToList();
            return new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = starSystems
            };
        }

        public IEnumerable<EmpireSystemsView> GetEmpireSystemsViews(IEnumerable<ObjectId> empireIds)
        {
            var empires = _empireCollection.Find(GetEmpiresByIds(empireIds)).ToList();
            var starSystemIds = empires.SelectMany(x => x.StarSystemsIds);
            var starSystems = _starSystemCollection.Find(GetStarSystemsByIds(starSystemIds)).ToList();
            return empires.Select(empire => new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = starSystems.Where(system => empire.StarSystemsIds.Contains(system.Id))
            });
        }

        public IEnumerable<EmpireBorderView> GetEmpireBorderViews(ObjectId empireId)
        {
            var empire = _empireCollection.Find(GetEmpireById(empireId)).Single();
            var empireSystems = _starSystemCollection.Find(GetStarSystemsByIds(empire.StarSystemsIds)).ToList();
            var borderingSystems = GetBorderStarSystems(empireSystems);
            var borderingEmpires = GetBorderingEmpires(borderingSystems);
            return borderingEmpires.Select(x => new EmpireBorderView
            {
                Empire = empire,
                EmpireSystems = empireSystems,
                BorderingEmpire = x.Empire,
                BorderingEmpireSystems = x.StarSystems
            });
        }

        public EmpireBorderView GetEmpireBorderView(ObjectId empireId, ObjectId borderingEmpireId)
        {
            var empire = _empireCollection.Find(GetEmpireById(empireId)).Single();
            var borderingEmpire = _empireCollection.Find(GetEmpireById(borderingEmpireId)).Single();
            var empireSystems = _starSystemCollection
                .Find(GetStarSystemsOnBorder(empire.StarSystemsIds, borderingEmpire.StarSystemsIds)).ToList();
            var borderingEmpireSystems = _starSystemCollection
                .Find(GetStarSystemsOnBorder(borderingEmpire.StarSystemsIds, empire.StarSystemsIds)).ToList();
            return new EmpireBorderView
            {
                Empire = empire,
                BorderingEmpire = borderingEmpire,
                EmpireSystems = empireSystems,
                BorderingEmpireSystems = borderingEmpireSystems
            };
        }

        public void TransferSystems(DeltaMetadata deltaMetadata, ObjectId senderId, ObjectId receiverId, IEnumerable<ObjectId> systemIds)
        {
            if (!systemIds.Any())
            {
                return;
            }
            
            _empireCollection.UpdateOne(GetEmpireById(senderId), RemoveSystems(systemIds));
            _empireCollection.UpdateOne(GetEmpireById(receiverId), AddSystems(systemIds));
            CreateDeltas(deltaMetadata, senderId, receiverId, systemIds);
        }

        public void ApplyMilitaryDamage(DeltaMetadata deltaMetadata, ObjectId empireId, double damage)
        {
            var empire = _empireCollection.Find(GetEmpireById(empireId)).Single();
            damage = Math.Min(damage, empire.Military);
            ApplyMilitaryProduction(deltaMetadata, empireId, -damage);
        }

        public void ApplyMilitaryProduction(DeltaMetadata deltaMetadata, ObjectId empireId, double increase)
        {
            _empireCollection.UpdateOne(GetEmpireById(empireId), ChangeMilitary(increase));
            var delta = new Delta<double>
            {
                DeltaType = DeltaType.EmpireMilitary,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = empireId,
                Value = increase
            };
            _deltaDoubleCollection.InsertOne(delta);

        }

        public void EmpireDefeated(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var empire = _empireCollection.Find(GetEmpireById(empireId)).Single();
            ApplyMilitaryDamage(deltaMetadata, empireId, empire.Military);
        }

        private void CreateDeltas(DeltaMetadata deltaMetadata, ObjectId senderId, ObjectId receiverId, IEnumerable<ObjectId> systemIds)
        {
            var addDeltas = systemIds.Select(x => new Delta<ObjectId>
            {
                DeltaType = DeltaType.EmpireSystemGain,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = receiverId,
                Value = x
            });
            var removeDeltas = systemIds.Select(x => new Delta<ObjectId>
            {
                DeltaType = DeltaType.EmpireSystemLoss,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = senderId,
                Value = x
            });
            _deltaObjectCollection.InsertMany(addDeltas.Concat(removeDeltas));
        }

        private IEnumerable<StarSystem> GetBorderStarSystems(IEnumerable<StarSystem> starSystems)
        {
            var borderingIds = starSystems
                .SelectMany(x => x.ConnectedSystemIds)
                .Where(borderingSystem => !starSystems.Any(system => system.Id == borderingSystem));
            return _starSystemCollection.Find(GetStarSystemsByIds(borderingIds)).ToList();
        }

        private IEnumerable<EmpireSystemsView> GetBorderingEmpires(IEnumerable<StarSystem> borderingStarSystems)
        {
            var borderingEmpires = _empireCollection.Find(GetEmpiresByStarSystemIds(borderingStarSystems.Select(x => x.Id))).ToList();
            return borderingEmpires.Select(empire => new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = borderingStarSystems.Where(system => empire.StarSystemsIds.Contains(system.Id))
            });
        }

        private FilterDefinition<Empire> GetEmpireById(ObjectId id) =>
            Builders<Empire>.Filter.Eq(empire => empire.Id, id);

        private FilterDefinition<Empire> GetEmpiresByIds(IEnumerable<ObjectId> empireIds) =>
            Builders<Empire>.Filter.In(empire => empire.Id, empireIds);
        
        private FilterDefinition<Empire> GetEmpiresByStarSystemIds(IEnumerable<ObjectId> starSystemIds) =>
            Builders<Empire>.Filter.AnyIn(empire => empire.StarSystemsIds, starSystemIds);
        
        private FilterDefinition<StarSystem> GetStarSystemsByIds(IEnumerable<ObjectId> starSystemIds) =>
            Builders<StarSystem>.Filter.In(starSystem => starSystem.Id, starSystemIds);
        
        private FilterDefinition<StarSystem> GetStarSystemsOnBorder(IEnumerable<ObjectId> systemIds, IEnumerable<ObjectId> borderingSystemIds) =>
            Builders<StarSystem>.Filter.In(system => system.Id, systemIds) &
            Builders<StarSystem>.Filter.AnyIn(system => system.ConnectedSystemIds, borderingSystemIds);
        
        private UpdateDefinition<Empire> AddSystems(IEnumerable<ObjectId> starSystemIds) =>
            Builders<Empire>.Update.PushEach(empire => empire.StarSystemsIds, starSystemIds);
        
        private UpdateDefinition<Empire> RemoveSystems(IEnumerable<ObjectId> starSystemIds) =>
            Builders<Empire>.Update.PullAll(empire => empire.StarSystemsIds, starSystemIds);
        
        private UpdateDefinition<Empire> ChangeMilitary(double delta) =>
            Builders<Empire>.Update.Inc(empire => empire.Military, delta);
    }
}