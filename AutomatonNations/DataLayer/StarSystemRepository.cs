using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AutomatonNations
{
    public class StarSystemRepository : IStarSystemRepository
    {
        private IMongoCollection<StarSystem> _starSystemCollection;
        private IMongoCollection<Delta<double>> _doubleDeltaCollection;
        private IMongoCollection<Simulation> _simulationRepository;
        private IMongoCollection<Sector> _sectorRepository;

        public StarSystemRepository(IDatabaseProvider databaseProvider)
        {
            _starSystemCollection = databaseProvider.Database.GetCollection<StarSystem>(Collections.StarSystems);
            _doubleDeltaCollection = databaseProvider.Database.GetCollection<Delta<double>>(Collections.Deltas);
            _simulationRepository = databaseProvider.Database.GetCollection<Simulation>(Collections.Simulations);
            _sectorRepository = databaseProvider.Database.GetCollection<Sector>(Collections.Sectors);
        }

        public IEnumerable<StarSystem> GetForSimulation(ObjectId simulationId)
        {
            var simulation = _simulationRepository.Find(Builders<Simulation>.Filter.Eq(sim => sim.Id, simulationId)).Single();
            var sector = _sectorRepository.Find(Builders<Sector>.Filter.Eq(sec => sec.Id, simulation.SectorId)).Single();
            return _starSystemCollection.Find(Builders<StarSystem>.Filter.In(system => system.Id, sector.StarSystemIds)).ToList();
        }

        public void ApplyDevelopment(IEnumerable<Delta<double>> deltas)
        {
            if (!deltas.Any())
            {
                return;
            }
            
            _doubleDeltaCollection.InsertMany(deltas);
            foreach (var delta in deltas)
            {
                var filter = GetById(delta.ReferenceId);
                var update = GetUpdateDevelopment(delta.Value);
                _starSystemCollection.UpdateOne(filter, update);
            }
        }

        public void ApplyDamage(IEnumerable<Delta<double>> deltas)
        {
            deltas = LimitDamage(deltas);
            ApplyDevelopment(deltas);
        }

        public ConnectedSystemsView GetConnectedSystems(ObjectId systemId)
        {
            var getSystem = GetById(systemId);
            var starSystem = _starSystemCollection.Find(getSystem).Single();
            var getConnected = GetInIds(starSystem.ConnectedSystemIds);
            var connectedSystems = _starSystemCollection.Find(getConnected).ToList();
            return new ConnectedSystemsView
            {
                StarSystem = starSystem,
                ConnectedSystems = connectedSystems
            };
        }

        private IEnumerable<Delta<double>> LimitDamage(IEnumerable<Delta<double>> deltas)
        {
            var starSystems = _starSystemCollection.Find(GetInIds(deltas.Select(x => x.ReferenceId))).ToList();
            return deltas.Select(delta =>
            {
                var starSystem = starSystems.Where(system => system.Id == delta.ReferenceId).Single();
                delta.Value = -Math.Min(delta.Value, starSystem.Development);
                return delta;
            });
        }

        private FilterDefinition<StarSystem> GetById(ObjectId id) =>
            Builders<StarSystem>.Filter.Eq(system => system.Id, id);
        
        private FilterDefinition<StarSystem> GetInIds(IEnumerable<ObjectId> ids) =>
           Builders<StarSystem>.Filter.In(system => system.Id, ids);

        private UpdateDefinition<StarSystem> GetUpdateDevelopment(double delta) =>
            Builders<StarSystem>.Update.Inc(system => system.Development, delta);
    }    
}