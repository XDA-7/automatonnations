using System;
using MongoDB.Bson;
using SimpleInjector;

namespace AutomatonNations
{
    class Program
    {
        public static void Main(string[] args)
        {
            var container = GetContainer();
            // var sectorGenerator = container.GetInstance<ISectorGenerator>();
            // sectorGenerator.CreateSector(20, 10, 3);
            var systemRepository = container.GetInstance<IStarSystemRepository>();
            var id = new ObjectId("5a8d683f1775e81df0de9a9a");
            var connectedSystems = systemRepository.GetConnectedSystems(id);
            foreach (var system in connectedSystems.ConnectedSystems)
            {
                Console.WriteLine(system.Coordinate.X);
                Console.WriteLine(system.Coordinate.Y);
            }
        }

        private static Container GetContainer()
        {
            var container = new Container();
            container.Register<IDatabaseProvider, DatabaseProvider>();
            container.Register<IDeltaApplier, DeltaApplier>();
            container.Register<IDeltaRepository, DeltaRepository>();
            container.Register<IDevelopmentCalculator, DevelopmentCalculator>();
            container.Register<IEconomicSimulator, EconomicSimulator>();
            container.Register<IRandom, RandomWrapper>();
            container.Register<ISectorGenerator, SectorGenerator>();
            container.Register<ISectorRepository, SectorRepository>();
            container.Register<ISimulationRepository, SimulationRepository>();
            container.Register<ISpatialOperations, SpatialOperations>();
            container.Register<IStarSystemRepository, StarSystemRepository>();
            return container;
        }
    }
}
