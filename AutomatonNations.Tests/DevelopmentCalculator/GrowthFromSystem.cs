using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_DevelopmentCalculator
{
    public class GrowthFromSystem
    {
        private Mock<IConfiguration> _configuration = new Mock<IConfiguration>();
        private Mock<ConnectedSystemsOnlyDelegate> _connectedSystemsOnlyDelegate = new Mock<ConnectedSystemsOnlyDelegate>();
        private IDevelopmentCalculator _developmentCalculator;

        private StarSystem _targetSystem;
        private StarSystem[] _connectedSystems;
        private StarSystem[] _systems;
        private EmpireSystemsView _empireView;

        public GrowthFromSystem()
        {
            _connectedSystems = new StarSystem[]
            {
                new StarSystem { Development = 300, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 200, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 400, Id = ObjectId.GenerateNewId() }
            };

            _targetSystem = new StarSystem
            {
                Development = 100.0,
                ConnectedSystemIds = new ObjectId[] { _connectedSystems[0].Id,_connectedSystems[1].Id, _connectedSystems[2].Id },
                Id = ObjectId.GenerateNewId()
            };

            _systems = new StarSystem[]
            {
                _connectedSystems[0],
                _connectedSystems[1],
                _connectedSystems[2],
                new StarSystem { Development = 500.0, Id = ObjectId.GenerateNewId() },
                new StarSystem { Development = 250.0, Id = ObjectId.GenerateNewId() }
            };

            _empireView = new EmpireSystemsView
            {
                StarSystems = _systems,
                Empire = new Empire { Alignment = new Alignment { Prosperity = 1.0 } }
            };

            var developmentCalculator = new DevelopmentCalculator(_configuration.Object);
            developmentCalculator.SetConnectedSystemsOnlyHook(_connectedSystemsOnlyDelegate.Object);
            _developmentCalculator = developmentCalculator;
        }


        [Fact]
        public void ReturnsEmptyCollectionIfSystemHasZeroDevelopment()
        {
            _targetSystem.Development = 0.0;
            var result = _developmentCalculator.GrowthFromSystem(_targetSystem, _empireView);
            Assert.Empty(result);
        }

        [Fact]
        public void ReturnsEmptyCollectionIfEmpireHasZeroProsperityFocus()
        {
            _empireView.Empire.Alignment.Prosperity = 0.0;
            var result = _developmentCalculator.GrowthFromSystem(_targetSystem, _empireView);
            Assert.Empty(result);
        }

        [Fact]
        public void CalculatesIncomeAsFunctionOfDevelopmentAndProsperity()
        {
            _empireView.Empire.Alignment.Prosperity = 0.30;
            _developmentCalculator.GrowthFromSystem(_targetSystem, _empireView);

            var income = _targetSystem.Development * Parameters.IncomeRate * 0.30;
            _connectedSystemsOnlyDelegate.Verify(x => x(It.IsAny<StarSystem>(), It.IsAny<IEnumerable<StarSystem>>(), income), Times.Once);
        }

        [Fact]
        public void CalculatesGrowthUsingOnlyConnectedSystems()
        {
            _developmentCalculator.GrowthFromSystem(_targetSystem, _empireView);
            _connectedSystemsOnlyDelegate.Verify(
                x => x(
                    It.IsAny<StarSystem>(),
                    It.Is<IEnumerable<StarSystem>>(
                        systems =>
                        systems.Contains(_connectedSystems[0]) &&
                        systems.Contains(_connectedSystems[1]) &&
                        systems.Contains(_connectedSystems[2]) &&
                        !systems.Contains(_systems[3]) &&
                        !systems.Contains(_systems[4])),
                    It.IsAny<double>()),
                Times.Once);
        }

        [Fact]
        public void DistributesGrowthBetweenAllSystemsProportionalToDevelopmenmt()
        {
            _configuration.Setup(x => x.DevelopmentCalculation)
                .Returns(DevelopmentCalculation.ProportionalDistribution);
            _developmentCalculator = new DevelopmentCalculator(_configuration.Object);
            var result = _developmentCalculator.GrowthFromSystem(_targetSystem, _empireView);

            var income = _targetSystem.Development * Parameters.IncomeRate;
            Assert.Contains(result, value => value.SystemId == _targetSystem.Id && value.Growth == 0.1 * income);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[0].Id && value.Growth == 0.3 * income);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[1].Id && value.Growth == 0.2 * income);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[2].Id && value.Growth == 0.4 * income);
        }

        [Fact]
        public void DistributesGrowthBetweenAllSystemEqually()
        {
            _configuration.Setup(x => x.DevelopmentCalculation)
                .Returns(DevelopmentCalculation.EqualDistribution);
            _developmentCalculator = new DevelopmentCalculator(_configuration.Object);
            var result = _developmentCalculator.GrowthFromSystem(_targetSystem, _empireView);

            var income = _targetSystem.Development * Parameters.IncomeRate;
            Assert.Contains(result, value => value.SystemId == _targetSystem.Id && value.Growth == 0.25 * income);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[0].Id && value.Growth == 0.25 * income);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[1].Id && value.Growth == 0.25 * income);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[2].Id && value.Growth == 0.25 * income);
        }

        [Fact]
        public void GivesFractionOfIncomeToSelfThenDistributesEqually()
        {
            _configuration.Setup(x => x.DevelopmentCalculation)
                .Returns(DevelopmentCalculation.SelfPriorityThenEqual);
            _developmentCalculator = new DevelopmentCalculator(_configuration.Object);
            var result = _developmentCalculator.GrowthFromSystem(_targetSystem, _empireView);

            var income = _targetSystem.Development * Parameters.IncomeRate;
            var incomeForNeighbours = (income * (1 - Parameters.IncomeReservedForSelf)) / 3.0;
            Assert.Contains(result, value => value.SystemId == _targetSystem.Id && value.Growth == Parameters.IncomeReservedForSelf * income);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[0].Id && value.Growth == incomeForNeighbours);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[1].Id && value.Growth == incomeForNeighbours);
            Assert.Contains(result, value => value.SystemId == _connectedSystems[2].Id && value.Growth == incomeForNeighbours);
        }
    }
}