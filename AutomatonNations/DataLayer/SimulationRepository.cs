using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class SimulationRepository : ISimulationRepository
    {
        private IMongoCollection<Simulation> _simulationCollection;
        private IMongoCollection<Sector> _sectorCollection;
        private IMongoCollection<StarSystem> _starSystemCollection;
        private IMongoCollection<Empire> _empireCollection;
        private IMongoCollection<War> _warCollection;

        public SimulationRepository(IDatabaseProvider databaseProvider)
        {
            _simulationCollection = databaseProvider.Database.GetCollection<Simulation>(Collections.Simulations);
            _sectorCollection = databaseProvider.Database.GetCollection<Sector>(Collections.Sectors);
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _empireCollection = databaseProvider.Database.GetCollection<Empire>(Collections.Empires);
            _warCollection = databaseProvider.Database.GetCollection<War>(Collections.Wars);
        }

        public Simulation GetSimulation(ObjectId simulationId) =>
            _simulationCollection.Find(GetSimulationById(simulationId)).Single();

        public SimulationView GetSimulationView(ObjectId simulationId)
        {
            var simulation = _simulationCollection.Find(GetSimulationById(simulationId)).Single();
            var sector = _sectorCollection.Find(GetSectorById(simulation.SectorId)).Single();
            var starSystems = _starSystemCollection.Find(GetStarSystemsInIds(sector.StarSystemIds)).ToList();
            var empires = _empireCollection.Find(GetEmpiresInIds(simulation.EmpireIds)).ToList();
            var wars = _warCollection.Find(GetWarsInIds(simulation.WarIds)).ToList();
            return new SimulationView
            {
                Simulation = simulation,
                StarSystems = starSystems,
                Empires = empires,
                Wars = wars
            };
        }

        public ObjectId Create(ObjectId sectorId, IEnumerable<ObjectId> empireIds)
        {
            var simulation = new Simulation
            {
                SectorId = sectorId,
                EmpireIds = empireIds,
                WarIds = new ObjectId[0]
            };
            _simulationCollection.InsertOne(simulation);
            return simulation.Id;
        }

        public void IncrementTick(ObjectId simulationId) =>
            _simulationCollection.UpdateOne(GetSimulationById(simulationId), IncrementTicksDef(1));

        private FilterDefinition<Simulation> GetSimulationById(ObjectId id) =>
            Builders<Simulation>.Filter.Eq(simulation => simulation.Id, id);
        
        private UpdateDefinition<Simulation> IncrementTicksDef(int ticks) =>
            Builders<Simulation>.Update.Inc(simulation => simulation.Ticks, ticks);
        
        private FilterDefinition<Sector> GetSectorById(ObjectId id) =>
            Builders<Sector>.Filter.Eq(sector => sector.Id, id);
        
        private FilterDefinition<StarSystem> GetStarSystemsInIds(IEnumerable<ObjectId> ids) =>
            Builders<StarSystem>.Filter.In(system => system.Id, ids);
        
        private FilterDefinition<Empire> GetEmpiresInIds(IEnumerable<ObjectId> ids) =>
            Builders<Empire>.Filter.In(empire => empire.Id, ids);

        private FilterDefinition<War> GetWarsInIds(IEnumerable<ObjectId> ids) =>
            Builders<War>.Filter.In(war => war.Id, ids);
    }
}