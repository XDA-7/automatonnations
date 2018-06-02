using System.Linq;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class LeaderUpdater : ILeaderUpdater
    {
        private IMongoCollection<Delta<LeaderUpdate>> _leaderUpdateCollection;
        private IEmpireRepository _empireRepository;

        public LeaderUpdater(IDatabaseProvider databaseProvider, IEmpireRepository empireRepository)
        {
            _leaderUpdateCollection = databaseProvider.Database.GetCollection<Delta<LeaderUpdate>>(Collections.Deltas);
            _empireRepository = empireRepository;
        }

        public void UpdateLeadersForSimulation(DeltaMetadata deltaMetadata, Simulation simulation)
        {
            var empires = _empireRepository.GetByIds(simulation.EmpireIds);
            var empireUpdates = empires.Select(empire => new EmpireLeaderUpdate
            {
                EmpireId = empire.Id,
                Leaders = empire.Leaders
            });

            _leaderUpdateCollection.InsertOne(new Delta<LeaderUpdate>
            {
                DeltaType = DeltaType.SimulationLeadersUpdated,
                SimulationId = deltaMetadata.SimulationId,
                Tick = deltaMetadata.Tick,
                Value = new LeaderUpdate
                {
                    EmpireUpdates = empireUpdates
                }
            });
        }
    }
}