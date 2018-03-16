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
            var simulator = container.GetInstance<ISimulator>();
            var simulation = new ObjectId("5aabc22a431b88206c92d92a");
            simulator.RunForTicks(simulation, 10);
            // var simulation = simulator.BeginSimulation(new BeginSimulationRequest(50, 10, 2));
            // Console.WriteLine(simulation.ToString());
        }

        private static Container GetContainer()
        {
            var container = new Container();
            container.Register<IDatabaseProvider, DatabaseProvider>();
            container.Register<IDeltaApplier, DeltaApplier>();
            container.Register<IDeltaRepository, DeltaRepository>();
            container.Register<IDevelopmentCalculator, DevelopmentCalculator>();
            container.Register<IEconomicSimulator, EconomicSimulator>();
            container.Register<IEmpireGenerator, EmpireGenerator>();
            container.Register<IEmpireRepository, EmpireRepository>();
            container.Register<IRandom, RandomWrapper>();
            container.Register<ISectorGenerator, SectorGenerator>();
            container.Register<ISectorRepository, SectorRepository>();
            container.Register<ISimulationRepository, SimulationRepository>();
            container.Register<ISimulator, Simulator>();
            container.Register<ISpatialOperations, SpatialOperations>();
            container.Register<IStarSystemRepository, StarSystemRepository>();
            return container;
        }
    }
}
