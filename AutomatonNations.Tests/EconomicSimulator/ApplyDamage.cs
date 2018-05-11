using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_EconomicSimulator
{
    public class ApplyDamage
    {
        private Mock<IStarSystemRepository> _starSystemRepository = new Mock<IStarSystemRepository>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private Mock<ILeaderRepository> _leaderRepository = new Mock<ILeaderRepository>();
        private Mock<IDevelopmentCalculator> _developmentCalculator = new Mock<IDevelopmentCalculator>();
        private Mock<IMilitaryCalculator> _militaryCalculator = new Mock<IMilitaryCalculator>();
        private IEconomicSimulator _economicSimulator;

        public ApplyDamage()
        {
            _economicSimulator = new EconomicSimulator(_starSystemRepository.Object, _empireRepository.Object, _leaderRepository.Object, _developmentCalculator.Object, _militaryCalculator.Object);
        }

        [Fact]
        public void CreatesDeltasWithCorrectValues()
        {
            var simulationId = ObjectId.GenerateNewId();
            var metadata = new DeltaMetadata(simulationId, 54);

            var empireSystems = new StarSystem[]
            {
                new StarSystem(),
                new StarSystem(),
                new StarSystem(),
                new StarSystem(),
                new StarSystem()
            };
            var borderingSystems = new StarSystem[]
            {
                new StarSystem(),
                new StarSystem(),
                new StarSystem(),
                new StarSystem(),
                new StarSystem()
            };

            var view = new EmpireBorderView
            {
                Empire = new Empire(),
                EmpireSystems = empireSystems,
                BorderingEmpire = new Empire(),
                BorderingEmpireSystems = borderingSystems
            };

            _economicSimulator.ApplyDamage(metadata, view, 324, 4224.56);

            foreach (var system in empireSystems)
            {
                _starSystemRepository.Verify(
                    x => x.ApplyDamage(
                        It.Is<IEnumerable<Delta<double>>>(
                            deltas =>
                            deltas.Any(
                                delta =>
                                delta.DeltaType == DeltaType.SystemDevelopment &&
                                delta.SimulationId == simulationId &&
                                delta.Tick == 54 &&
                                delta.ReferenceId == system.Id &&
                                delta.Value == 324))),
                    Times.Once);
            }

            foreach (var system in borderingSystems)
            {
                _starSystemRepository.Verify(
                    x => x.ApplyDamage(
                        It.Is<IEnumerable<Delta<double>>>(
                            deltas =>
                            deltas.Any(
                                delta =>
                                delta.DeltaType == DeltaType.SystemDevelopment &&
                                delta.SimulationId == simulationId &&
                                delta.Tick == 54 &&
                                delta.ReferenceId == system.Id &&
                                delta.Value == 4224.56))),
                    Times.Once);
            }
        }
    }
}