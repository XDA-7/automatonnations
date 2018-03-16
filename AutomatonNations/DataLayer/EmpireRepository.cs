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

        public EmpireRepository(IDatabaseProvider databaseProvider)
        {
            _empireCollection = databaseProvider.Database.GetCollection<Empire>(Collections.Empires);
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
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

        private FilterDefinition<Empire> GetEmpiresByIds(IEnumerable<ObjectId> empireIds) =>
            Builders<Empire>.Filter.In(empire => empire.Id, empireIds);
        
        private FilterDefinition<StarSystem> GetStarSystemsByIds(IEnumerable<ObjectId> starSystemIds) =>
            Builders<StarSystem>.Filter.In(starSystem => starSystem.Id, starSystemIds);
    }
}