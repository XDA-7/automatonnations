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
            // var simulation = simulator.BeginSimulation(new BeginSimulationRequest(50, 10, 2, 10000));
            // var simulation = new ObjectId("5aabe4f9252ff821f8f00659");
            // simulator.RunForTicks(simulation, 10);
            // var original = simulator.GetAtTick(simulation, 3);
            // foreach (var system in original.StarSystems)
            // {
            //     Console.WriteLine(system.Development);
            // }

            // Console.WriteLine(simulation.ToString());

            var empireRepository = container.GetInstance<IEmpireRepository>();
            // var empireId = new ObjectId("5aabe4f9252ff821f8f00637");
            // var borderViews = empireRepository.GetEmpireBorderViews(empireId);
            // foreach (var borderView in borderViews)
            // {
            //     Console.WriteLine("Empire: " + borderView.Empire.Id);
            //     foreach (var system in borderView.EmpireSystems)
            //     {
            //         Console.WriteLine(system.Id);
            //     }

            //     Console.WriteLine("Bordering empire: " + borderView.BorderingEmpire.Id);
            //     foreach (var borderSystem in borderView.BorderingEmpireSystems)
            //     {
            //         Console.WriteLine(borderSystem.Id);
            //     }

            //     Console.WriteLine();
            // }

            // var sender = empireRepository.GetEmpireSystemsViews(new ObjectId[] { new ObjectId("5aabe4f9252ff821f8f00658") }).Single();
            // var receiver = empireRepository.GetEmpireSystemsViews(new ObjectId[] { new ObjectId("5aabe4f9252ff821f8f00637") }).Single();
            // empireRepository.TransferSystems(new DeltaMetadata(ObjectId.Empty, 0), sender.Empire, receiver.Empire, new ObjectId[] { new ObjectId("5aabe4f8252ff821f8f00625") });
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
