﻿using System;
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
            WipeDatabase();
            _container = GetContainer();
            var simulator = _container.GetInstance<ISimulator>();

            var simId = simulator.BeginSimulation(new BeginSimulationRequest(100, 20, 5, 1000));
            simulator.RunForTicks(simId, 10);

            // PrintSystemDevelopments(new ObjectId("5ab77471320ef21a54039704"));
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
            container.Register<IDatabaseProvider, DatabaseProvider>();
            container.Register<IDeltaApplier, DeltaApplier>();
            container.Register<IDeltaRepository, DeltaRepository>();
            container.Register<IDevelopmentCalculator, DevelopmentCalculator>();
            container.Register<IDiplomacyCalculator, DiplomacyCalculator>();
            container.Register<IDiplomacySimulator, DiplomacySimulator>();
            container.Register<IEconomicSimulator, EconomicSimulator>();
            container.Register<IEmpireGenerator, EmpireGenerator>();
            container.Register<IEmpireRepository, EmpireRepository>();
            container.Register<IMilitaryCalculator, MilitaryCalculator>();
            container.Register<IMilitarySimulator, MilitarySimulator>();
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
