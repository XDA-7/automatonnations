using System;
using System.Linq;
using MongoDB.Bson;
using SimpleInjector;

namespace AutomatonNations
{
    class Program
    {
        private static Container _container;

        public static void Main(string[] args)
        {
            // WipeDatabase();
            _container = GetContainer();
            
            var simulator = _container.GetInstance<ISimulator>();
            // var simId = simulator.BeginSimulation(new BeginSimulationRequest(100, 20, 2, 1000));
            // simulator.RunForTicks(simId, 40);

            var simId = new ObjectId("5ae42b656effba227c76ef56");
            var displayGenerator = _container.GetInstance<IDisplayGenerator>();
            displayGenerator.CreateForSimulation(simId);
            Console.WriteLine(simId);

            // var view = simulator.GetAtTick(simId, 18);
            // var systemIds = view.Empires.SelectMany(empire => empire.StarSystemsIds);
            // var duplicateCount = new System.Collections.Generic.Dictionary<ObjectId, int>();
            // foreach (var id in systemIds)
            // {
            //     if (duplicateCount.ContainsKey(id))
            //     {
            //         duplicateCount[id] += 1;
            //     }
            //     else
            //     {
            //         duplicateCount.Add(id, 1);
            //     }
            // }

            // Console.WriteLine(duplicateCount.Count);
            // foreach(var pair in duplicateCount)
            // {
            //     if (pair.Value > 1)
            //     {
            //         Console.WriteLine(pair.Key);
            //         Console.WriteLine(pair.Value);
            //     }
            // }
        }

        private static void WipeDatabase()
        {
            var client = new MongoDB.Driver.MongoClient();
            client.DropDatabase("AutomatonNations");
        }

        private static void PrintSystemDevelopments(ObjectId simulationId)
        {
            var systemRepository = _container.GetInstance<IStarSystemRepository>();
            var systems = systemRepository.GetForSimulation(simulationId);
            foreach (var system in systems)
            {
                Console.WriteLine(system.Development);
            }
        }

        private static Container GetContainer()
        {
            var container = new Container();
            container.Register<IConfiguration, Configuration>();
            container.Register<IDatabaseProvider, DatabaseProvider>();
            container.Register<IDeltaApplier, DeltaApplier>();
            container.Register<IDeltaRepository, DeltaRepository>();
            container.Register<IDevelopmentCalculator, DevelopmentCalculator>();
            container.Register<IDiplomacyCalculator, DiplomacyCalculator>();
            container.Register<IDiplomacySimulator, DiplomacySimulator>();
            container.Register<IDisplayGenerator, DisplayGenerator>();
            container.Register<IEconomicSimulator, EconomicSimulator>();
            container.Register<IEmpireGenerator, EmpireGenerator>();
            container.Register<IEmpireRepository, EmpireRepository>();
            container.Register<IMilitaryCalculator, MilitaryCalculator>();
            container.Register<IMilitarySimulator, MilitarySimulator>();
            container.Register<IPresentationRepository, PresentationRepository>();
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
