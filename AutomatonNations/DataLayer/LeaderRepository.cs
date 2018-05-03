using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class LeaderRepository : ILeaderRepository
    {
        private IMongoCollection<Empire> _empireCollection;

        public LeaderRepository(IDatabaseProvider databaseProvider)
        {
            _empireCollection = databaseProvider.Database.GetCollection<Empire>(Collections.Empires);
        }

        public void SetLeadersForEmpire(ObjectId empireId, IEnumerable<Leader> leaders)
        {
            _empireCollection.UpdateOne(GetEmpireById(empireId), SetLeadersInEmpire(leaders));
        }

        private FilterDefinition<Empire> GetEmpireById(ObjectId id) =>
            Builders<Empire>.Filter.Eq(empire => empire.Id, id);
        
        private UpdateDefinition<Empire> SetLeadersInEmpire(IEnumerable<Leader> leaders) =>
            Builders<Empire>.Update.Set(empire => empire.Leaders, leaders);
    }
}