using System;
using System.Linq;
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
            container.Register<IMilitaryCalculator, MilitaryCalculator>();
            container.Register<IRandom, RandomWrapper>();
            container.Register<ISectorGenerator, SectorGenerator>();
            container.Register<ISectorRepository, SectorRepository>();
            container.Register<ISimulationRepository, SimulationRepository>();
            container.Register<ISimulator, Simulator>();
            container.Register<ISpatialCalculator, SpatialCalculator>();
            container.Register<IStarSystemRepository, StarSystemRepository>();
            container.Register<IWarRepository, WarRepository>();
            return container;
        }
    }
}
