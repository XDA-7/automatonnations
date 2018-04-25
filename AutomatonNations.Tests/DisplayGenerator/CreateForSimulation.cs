using System.Collections.Generic;
using System.Linq;
using AutomatonNations.Presentation;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_DisplayGenerator
{
    public class CreateForSimulation
    {
        private Mock<ISimulator> _simulator = new Mock<ISimulator>();
        private Mock<IPresentationRepository> _presentationRepository = new Mock<IPresentationRepository>();
        private IDisplayGenerator _displayGenerator;

        private Simulation _simulation = new Simulation
        {
            Ticks = 1
        };
        private StarSystem[] _starSystems = new StarSystem[]
        {
            new StarSystem { Id = ObjectId.GenerateNewId(), Development = 23.5, Coordinate = new Coordinate { X = 34, Y = 55 } },
            new StarSystem { Id = ObjectId.GenerateNewId(), Development = 56.3, Coordinate = new Coordinate { X = 12, Y = 66 } },
            new StarSystem { Id = ObjectId.GenerateNewId(), Development = 473.3, Coordinate = new Coordinate { X = 3, Y = 21 } },
            new StarSystem { Id = ObjectId.GenerateNewId(), Development = 108.0, Coordinate = new Coordinate { X = 9, Y = 85 } },
            new StarSystem { Id = ObjectId.GenerateNewId(), Development = 574.56, Coordinate = new Coordinate { X = 58, Y = 11 } },
        };
        private Empire[] _empires = null;
        private SimulationView _simulationView = null;

        public CreateForSimulation()
        {
            _displayGenerator = new DisplayGenerator(_simulator.Object, _presentationRepository.Object);

            _starSystems[0].ConnectedSystemIds = new ObjectId[] { _starSystems[1].Id, _starSystems[2].Id };
            _starSystems[1].ConnectedSystemIds = new ObjectId[] { _starSystems[1].Id, _starSystems[4].Id, _starSystems[3].Id };
            _starSystems[2].ConnectedSystemIds = new ObjectId[] { _starSystems[4].Id };
            _starSystems[3].ConnectedSystemIds = new ObjectId[] { _starSystems[0].Id, _starSystems[1].Id };
            _starSystems[4].ConnectedSystemIds = new ObjectId[] { _starSystems[1].Id };
            _empires = new Empire[]
            {
                new Empire { Id = ObjectId.GenerateNewId(), StarSystemsIds = new ObjectId[] { _starSystems[1].Id, _starSystems[3].Id } },
                new Empire { Id = ObjectId.GenerateNewId(), StarSystemsIds = new ObjectId[0] },
                new Empire { Id = ObjectId.GenerateNewId(), StarSystemsIds = new ObjectId[] { _starSystems[0].Id, _starSystems[2].Id } },
                new Empire { Id = ObjectId.GenerateNewId(), StarSystemsIds = new ObjectId[] { _starSystems[1].Id, _starSystems[2].Id, _starSystems[4].Id } }
            };
            _simulationView = new SimulationView
            {
                Simulation = _simulation,
                StarSystems = _starSystems,
                Empires = _empires
            };
            _simulator
                .Setup(x => x.GetAtTick(It.IsAny<ObjectId>(), It.IsAny<int>()))
                .Returns(_simulationView);
            _simulator
                .Setup(x => x.GetLatest(It.IsAny<ObjectId>()))
                .Returns(_simulationView);
        }

        [Fact]
        public void SavesSectorForEachTickInRange()
        {
            _simulation.Ticks = 500;
            _displayGenerator.CreateForSimulation(ObjectId.GenerateNewId(), 22, 304);
            for (var i = 22; i <= 304; i++)
            {
                _presentationRepository.Verify(
                    x => x.Create(It.Is<Presentation.Sector>(sector => sector.Tick == i)),
                    Times.Once);
            }
        }

        [Fact]
        public void SavesSectorForAllTicksIfNoRangePassed()
        {
            _simulation.Ticks = 500;
            _displayGenerator.CreateForSimulation(ObjectId.GenerateNewId());
            _presentationRepository.Verify(x => x.Create(It.IsAny<Presentation.Sector>()), Times.Exactly(500));
        }

        [Fact]
        public void SavesBasicSystemData()
        {
            _displayGenerator.CreateForSimulation(ObjectId.GenerateNewId());
            _presentationRepository.Verify(
                x => x.Create(It.Is<Presentation.Sector>(
                    sector =>
                    BasicDataMatches(_starSystems[0], sector.StarSystems.ToArray()[0]) &&
                    BasicDataMatches(_starSystems[1], sector.StarSystems.ToArray()[1]) &&
                    BasicDataMatches(_starSystems[2], sector.StarSystems.ToArray()[2]) &&
                    BasicDataMatches(_starSystems[3], sector.StarSystems.ToArray()[3]) &&
                    BasicDataMatches(_starSystems[4], sector.StarSystems.ToArray()[4]))),
                Times.Once);
        }

        [Fact]
        public void SavesEmpireControllingSystem()
        {
            _displayGenerator.CreateForSimulation(ObjectId.GenerateNewId());
            _presentationRepository.Verify(
                x => x.Create(It.Is<Presentation.Sector>(
                    sector =>
                    sector.StarSystems.ToArray()[0].EmpireIds.Contains(_empires[2].Id) &&
                    sector.StarSystems.ToArray()[1].EmpireIds.Contains(_empires[0].Id) &&
                    sector.StarSystems.ToArray()[1].EmpireIds.Contains(_empires[3].Id) &&
                    sector.StarSystems.ToArray()[2].EmpireIds.Contains(_empires[2].Id) &&
                    sector.StarSystems.ToArray()[2].EmpireIds.Contains(_empires[3].Id) &&
                    sector.StarSystems.ToArray()[3].EmpireIds.Contains(_empires[0].Id) &&
                    sector.StarSystems.ToArray()[4].EmpireIds.Contains(_empires[3].Id))),
                Times.Once);
        }

        [Fact]
        public void SavesConnectionsBetweenSystems()
        {
            _displayGenerator.CreateForSimulation(ObjectId.GenerateNewId());
            _presentationRepository.Verify(
                x => x.Create(It.Is<Presentation.Sector>(
                    sector =>
                    sector.StarSystemConnections.Count() == 9 &&
                    ContainsCoordinates(sector, 34, 55, 12, 66) &&
                    ContainsCoordinates(sector, 34, 55, 3, 21) &&
                    ContainsCoordinates(sector, 12, 66, 12, 66) &&
                    ContainsCoordinates(sector, 12, 66, 58, 11) &&
                    ContainsCoordinates(sector, 12, 66, 9, 85) &&
                    ContainsCoordinates(sector, 3, 21, 58, 11) &&
                    ContainsCoordinates(sector, 9, 85, 34, 55) &&
                    ContainsCoordinates(sector, 9, 85, 12, 66) &&
                    ContainsCoordinates(sector, 58, 11, 12, 66))),
                Times.Once);
        }

        private bool BasicDataMatches(StarSystem starSystem, Presentation.StarSystem presentationSystem)
        {
            return starSystem.Coordinate.X == presentationSystem.Coordinate.X &&
                starSystem.Coordinate.Y == presentationSystem.Coordinate.Y &&
                starSystem.Development == presentationSystem.Development;
        }

        private bool ContainsCoordinates(Presentation.Sector sector, int sourceX, int sourceY, int destX, int destY)
        {
            return sector.StarSystemConnections.Where(
                connection =>
                connection.Source.X == sourceX &&
                connection.Source.Y == sourceY &&
                connection.Destination.X == destX &&
                connection.Destination.Y == destY)
            .Count() == 1;
        }
    }
}