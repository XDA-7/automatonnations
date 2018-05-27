using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class LeaderRepository : ILeaderRepository
    {
        private IMongoCollection<Empire> _empireCollection;
        private IMongoCollection<Delta<IEnumerable<Leader>>> _leaderDeltaCollection;

        public LeaderRepository(IDatabaseProvider databaseProvider)
        {
            _empireCollection = databaseProvider.Database.GetCollection<Empire>(Collections.Empires);
            _leaderDeltaCollection = databaseProvider.Database.GetCollection<Delta<IEnumerable<Leader>>>(Collections.Deltas);
        }

        public void SetLeadersForEmpire(DeltaMetadata deltaMetadata, ObjectId empireId, IEnumerable<Leader> leaders)
        {
            _empireCollection.UpdateOne(GetEmpireById(empireId), SetLeadersInEmpire(leaders));
            //  TODO: we won't be able to get back to provious leader setups this way, we'll run a single delta at the end of each turn instead
            _leaderDeltaCollection.InsertOne(new Delta<IEnumerable<Leader>>
            {
                DeltaType = DeltaType.EmpireLeadersUpdated,
                Tick = deltaMetadata.Tick,
                Value = leaders,
                ReferenceId = empireId,
                SimulationId = deltaMetadata.SimulationId
            });
        }

        private FilterDefinition<Empire> GetEmpireById(ObjectId id) =>
            Builders<Empire>.Filter.Eq(empire => empire.Id, id);
        
        private UpdateDefinition<Empire> SetLeadersInEmpire(IEnumerable<Leader> leaders) =>
            Builders<Empire>.Update.Set(empire => empire.Leaders, leaders);
    }
}