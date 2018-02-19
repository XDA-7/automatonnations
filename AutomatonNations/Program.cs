using System;
using SimpleInjector;

namespace AutomatonNations
{
    class Program
    {
        public static void Main(string[] args)
        {
            var container = GetContainer();
            var sectorGenerator = container.GetInstance<ISectorGenerator>();
            sectorGenerator.CreateSector(20, 10, 3);
        }

        private static Container GetContainer()
        {
            var container = new Container();
            container.Register<IDatabaseProvider, DatabaseProvider>();
            container.Register<IRandom, RandomWrapper>();
            container.Register<ISectorGenerator, SectorGenerator>();
            container.Register<ISectorRepository, SectorRepository>();
            container.Register<ISpatialOperations, SpatialOperations>();
            return container;
        }
    }
}
