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

        public EmpireRepository(IDatabaseProvider databaseProvider)
        {
            _empireCollection = databaseProvider.Database.GetCollection<Empire>(Collections.Empires);
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _deltaObjectCollection = databaseProvider.Database.GetCollection<Delta<ObjectId>>(Collections.Deltas);
            _deltaDoubleCollection = databaseProvider.Database.GetCollection<Delta<double>>(Collections.Deltas);        }

        public IEnumerable<ObjectId> Create(IEnumerable<CreateEmpireRequest> requests)
        {
            if (!requests.Any())
            {
                return new ObjectId[0];
            }

            var empires = requests.Select(x => new Empire
            {
                Alignment = x.Alignment,
                StarSystemsIds = x.StarSystemIds
            }).ToArray();

            _empireCollection.InsertMany(empires);
            return empires.Select(x => x.Id);
        }

        public Empire GetById(ObjectId empireId) =>
            _empireCollection.Find(GetEmpireById(empireId)).Single();

        public EmpireSystemsView GetEmpireSystemsView(ObjectId empireId)
        {
            var empire = _empireCollection.Find(GetEmpireById(empireId)).Single();
            var starSystems = _starSystemCollection.Find(GetStarSystemsByIds(empire.StarSystemsIds)).ToEnumerable();
            return new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = starSystems
            };
        }

        public IEnumerable<EmpireSystemsView> GetEmpireSystemsViews(IEnumerable<ObjectId> empireIds)
        {
            var empires = _empireCollection.Find(GetEmpiresByIds(empireIds)).ToEnumerable();
            var starSystemIds = empires.SelectMany(x => x.StarSystemsIds);
            var starSystems = _starSystemCollection.Find(GetStarSystemsByIds(starSystemIds)).ToEnumerable();
            return empires.Select(empire => new EmpireSystemsView
            {
                Empire = empire,
                StarSystems = starSystems.Where(system => empire.StarSystemsIds.Contains(system.Id))
            });
        }

        public IEnumerable<EmpireBorderView> GetEmpireBorderViews(ObjectId empireId)
        {
            var empire = _empireCollection.Find(GetEmpireById(empireId)).Single();
            var empireSystems = _starSystemCollection.Find(GetStarSystemsByIds(empire.StarSystemsIds)).ToEnumerable();
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
                .Find(GetStarSystemsOnBorder(empire.StarSystemsIds, borderingEmpire.StarSystemsIds)).ToEnumerable();
            var borderingEmpireSystems = _starSystemCollection
                .Find(GetStarSystemsOnBorder(borderingEmpire.StarSystemsIds, empire.StarSystemsIds)).ToEnumerable();
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
            _empireCollection.UpdateOne(GetEmpireById(senderId), RemoveSystems(systemIds));
            _empireCollection.UpdateOne(GetEmpireById(receiverId), AddSystems(systemIds));
            CreateDeltas(deltaMetadata, senderId, receiverId, systemIds);
        }

        public void ApplyMilitaryDamage(DeltaMetadata deltaMetadata, ObjectId empireId, double damage)
        {
            _empireCollection.UpdateOne(GetEmpireById(empireId), ChangeMilitary(-damage));
            var delta = new Delta<double>
            {
                DeltaType = DeltaType.EmpireMilitary,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = empireId,
                Value = -damage
            };
            _deltaDoubleCollection.InsertOne(delta);
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
            return _starSystemCollection.Find(GetStarSystemsByIds(borderingIds)).ToEnumerable();
        }

        private IEnumerable<EmpireSystemsView> GetBorderingEmpires(IEnumerable<StarSystem> borderingStarSystems)
        {
            var borderingEmpires = _empireCollection.Find(GetEmpiresByStarSystemIds(borderingStarSystems.Select(x => x.Id))).ToEnumerable();
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