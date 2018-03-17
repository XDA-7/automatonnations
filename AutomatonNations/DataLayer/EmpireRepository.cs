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

        public EmpireRepository(IDatabaseProvider databaseProvider)
        {
            _empireCollection = databaseProvider.Database.GetCollection<Empire>(Collections.Empires);
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _deltaObjectCollection = databaseProvider.Database.GetCollection<Delta<ObjectId>>(Collections.Deltas);
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
                StarSystemsIds = x.StarSystemIds
            }).ToArray();

            _empireCollection.InsertMany(empires);
            return empires.Select(x => x.Id);
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
            var borderingSystems = GetBorderingStarSystems(empireSystems);
            var borderingEmpires = GetBorderingEmpires(borderingSystems);
            return borderingEmpires.Select(x => new EmpireBorderView
            {
                Empire = empire,
                EmpireSystems = empireSystems,
                BorderingEmpire = x.Empire,
                BorderingEmpireSystems = x.StarSystems
            });
        }

        public void TransferSystems(DeltaMetadata deltaMetadata, Empire sender, Empire receiver, IEnumerable<ObjectId> systemIds)
        {
            _empireCollection.UpdateOne(GetEmpireById(sender.Id), RemoveSystems(systemIds));
            _empireCollection.UpdateOne(GetEmpireById(receiver.Id), AddSystems(systemIds));
            CreateDeltas(deltaMetadata, sender, receiver, systemIds);
        }

        private void CreateDeltas(DeltaMetadata deltaMetadata, Empire sender, Empire receiver, IEnumerable<ObjectId> systemIds)
        {
            var addDeltas = systemIds.Select(x => new Delta<ObjectId>
            {
                DeltaType = DeltaType.EmpireSystemGain,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = receiver.Id,
                Value = x
            });
            var removeDeltas = systemIds.Select(x => new Delta<ObjectId>
            {
                DeltaType = DeltaType.EmpireSystemLoss,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                ReferenceId = sender.Id,
                Value = x
            });
            _deltaObjectCollection.InsertMany(addDeltas.Concat(removeDeltas));
        }

        private IEnumerable<StarSystem> GetBorderingStarSystems(IEnumerable<StarSystem> starSystems)
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
        
        private UpdateDefinition<Empire> AddSystems(IEnumerable<ObjectId> starSystemIds) =>
            Builders<Empire>.Update.PushEach(empire => empire.StarSystemsIds, starSystemIds);
        
        private UpdateDefinition<Empire> RemoveSystems(IEnumerable<ObjectId> starSystemIds) =>
            Builders<Empire>.Update.PullAll(empire => empire.StarSystemsIds, starSystemIds);
    }
}