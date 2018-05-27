using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_LeaderSimulator
{
    public class RunEmpire
    {
        private Mock<ILeaderRepository> _leaderRepository = new Mock<ILeaderRepository>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private Mock<IEmpireGenerator> _empireGenerator = new Mock<IEmpireGenerator>();
        private Mock<IRandom> _random = new Mock<IRandom>();
        private ILeaderSimulator _leaderSimulator;

        private ObjectId[] _systemIds = new ObjectId[]
        {
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId()
        };
        private StarSystem[] _starSystems = null;
        private Leader[] _leaders = null;
        private Empire _empire = null;
        private EmpireSystemsView _empireView = null;

        public RunEmpire()
        {
            _leaderSimulator = new LeaderSimulator(_leaderRepository.Object, _empireRepository.Object, _empireGenerator.Object, _random.Object);
            _starSystems = new StarSystem[]
            {
                new StarSystem { Id = _systemIds[0], ConnectedSystemIds = new ObjectId[] { _systemIds[1], _systemIds[2], _systemIds[3] } },
                new StarSystem { Id = _systemIds[1], ConnectedSystemIds = new ObjectId[] { _systemIds[0], _systemIds[3] } },
                new StarSystem { Id = _systemIds[2], ConnectedSystemIds = new ObjectId[] { _systemIds[0], _systemIds[3] } },
                new StarSystem { Id = _systemIds[3], ConnectedSystemIds = new ObjectId[] { _systemIds[0], _systemIds[1], _systemIds[2], _systemIds[4] } },
                new StarSystem { Id = _systemIds[4], ConnectedSystemIds = new ObjectId[] { _systemIds[3] } }
            };
            _leaders = new Leader[]
            {
                new Leader { StarSystemIds = new ObjectId[0], SystemLimit = 2, EmpireLeader = true },
                new Leader { StarSystemIds = new ObjectId[] { _systemIds[2] }, SystemLimit = 4 },
                new Leader { StarSystemIds = new ObjectId[] { _systemIds[4] }, SystemLimit = 3 }
            };
            _empire = new Empire
            {
                Id = ObjectId.GenerateNewId(),
                StarSystemsIds = _systemIds,
                Leaders = _leaders
            };
            _empireView = new EmpireSystemsView
            {
                Empire = _empire,
                StarSystems = _starSystems
            };
            _empireRepository
                .Setup(x => x.GetEmpireSystemsView(It.IsAny<ObjectId>()))
                .Returns(_empireView);
            
            _random
                .Setup(x => x.ShuffleElements(_leaders))
                .Returns(new Leader[] { _leaders[0], _leaders[1], _leaders[2] });
            _random
                .Setup(x => x.GetRandomElement(It.Is<IEnumerable<ObjectId>>(
                    ids =>
                    ids.Count() == 3 &&
                    ids.Contains(_systemIds[0]) &&
                    ids.Contains(_systemIds[1]) &&
                    ids.Contains(_systemIds[3]))))
                .Returns(_systemIds[1]);
            _random
                .Setup(x => x.GetRandomElement(It.Is<IEnumerable<ObjectId>>(
                    ids =>
                    ids.Count() == 1 &&
                    ids.Contains(_systemIds[3]))))
                .Returns(_systemIds[3]);
            _random
                .Setup(x => x.GetRandomElement(It.Is<IEnumerable<ObjectId>>(
                    ids =>
                    ids.Count() == 2 &&
                    ids.Contains(_systemIds[0]) &&
                    ids.Contains(_systemIds[3]))))
                .Returns(_systemIds[0]);
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 5))
                .Returns(new double[] { 1.0, 1.0, 1.0, 1.0, 1.0 });
        }

        [Fact]
        public void AssignsSystemToLeadersBelowTheirLimitWhenAdjacentSystemsAvailable()
        {
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            VerifyUpdate(
                leaders =>
                leaders.ToArray()[1].StarSystemIds.Count() == 2 &&
                leaders.ToArray()[1].StarSystemIds.Contains(_systemIds[0]) &&
                leaders.ToArray()[1].StarSystemIds.Contains(_systemIds[2]) &&
                leaders.ToArray()[2].StarSystemIds.Count() == 2 &&
                leaders.ToArray()[2].StarSystemIds.Contains(_systemIds[4]) &&
                leaders.ToArray()[2].StarSystemIds.Contains(_systemIds[3]));
        }

        [Fact]
        public void AssignsAnyAvailableSystemToLeadersWithoutAnySystems()
        {
            _random
                .Setup(x => x.GetRandomElement(It.Is<IEnumerable<ObjectId>>(
                    ids =>
                    ids.Count() == 5)))
                .Returns(_systemIds[0]);
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            VerifyUpdate(
                leaders =>
                leaders.ToArray()[0].StarSystemIds.Contains(_systemIds[1]));
        }

        [Fact]
        public void DoesNotAssignSystemsBelongingToOtherLeaders()
        {
            _leaders[0].StarSystemIds = new ObjectId[] { _systemIds[1] };
            _leaders[1].StarSystemIds = new ObjectId[] { _systemIds[0], _systemIds[2] };
            _leaders[2].StarSystemIds = new ObjectId[] { _systemIds[3], _systemIds[4] };
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            _random.Verify(x => x.GetRandomElement(It.IsAny<IEnumerable<ObjectId>>()), Times.Never);
            VerifyUpdate(
                leaders =>
                leaders.Count() == 3 &&
                leaders.ToArray()[0].StarSystemIds.Count() == 1 &&
                leaders.ToArray()[1].StarSystemIds.Count() == 2 &&
                leaders.ToArray()[2].StarSystemIds.Count() == 2);
        }

        [Fact]
        public void DoesNotAssignSystemsToLeadersAtTheirLimit()
        {
            _leaders[2].SystemLimit = 1;
            _random
                .Setup(x => x.ShuffleElements(It.Is<IEnumerable<Leader>>(
                    leaders =>
                    leaders.Count() == 2)))
                .Returns(new Leader[] { _leaders[0], _leaders[1] });
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            VerifyUpdate(
                leaders =>
                leaders.ToArray()[2].StarSystemIds.Count() == 1 &&
                leaders.ToArray()[2].StarSystemIds.Contains(_systemIds[4]));
        }

        [Fact]
        public void IncrementsLeaderSystemLimitIfRandomNumberBelowThreshold()
        {
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 5))
                .Returns(new double[] { 1.0, 0.0, 0.0, 1.0, 1.0 });
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            VerifyUpdate(
                leaders =>
                leaders.ToArray()[0].SystemLimit == 2 &&
                leaders.ToArray()[1].SystemLimit == 5 &&
                leaders.ToArray()[2].SystemLimit == 4);
        }

        [Fact]
        public void DoesNotIncrementLeaderSystemLimitIfMaxLimitReached()
        {
            _leaders[1].SystemLimit = Parameters.LeaderMaxSystemLimit;
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 5))
                .Returns(new double[] { 1.0, 0.0, 1.0, 1.0, 1.0 });
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            VerifyUpdate(
                leaders =>
                leaders.ToArray()[1].SystemLimit == Parameters.LeaderMaxSystemLimit);
        }

        [Fact]
        public void AssignsAvailableSystemToLeaderIfSystemLimitIncreasedOnTurn()
        {
            _leaders[1].SystemLimit = 1;
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 5))
                .Returns(new double[] { 1.0, 0.0, 1.0, 1.0, 1.0 });
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            VerifyUpdate(
                leaders =>
                leaders.ToArray()[1].StarSystemIds.Count() == 2 &&
                leaders.ToArray()[1].StarSystemIds.Contains(_systemIds[0]) &&
                leaders.ToArray()[1].StarSystemIds.Contains(_systemIds[2]));
        }

        [Fact]
        public void SecedesLeaderFromEmpireIfRandomNumberBelowThreshold()
        {
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 5))
                .Returns(new double[] { 1.0, 1.0, 1.0, 1.0, 0.0 });
            _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            _empireGenerator.Verify(x => x.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[2]), Times.Once);
        }

        [Fact]
        public void WillNeverSecedeEmpireLeader()
        {
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 5))
                .Returns(new double[] { 1.0, 1.0, 1.0, 0.0, 0.0 });

                _leaders[0].EmpireLeader = true;
                _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
                _empireGenerator.Verify(x => x.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[1]), Times.Once);
                _empireGenerator.Verify(x => x.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[2]), Times.Once);
                _leaders[0].EmpireLeader = false;
                _empireGenerator.ResetCalls();

                _leaders[1].EmpireLeader = true;
                _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
                _empireGenerator.Verify(x => x.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[0]), Times.Once);
                _empireGenerator.Verify(x => x.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[2]), Times.Once);
                _leaders[1].EmpireLeader = false;
                _empireGenerator.ResetCalls();

                _leaders[2].EmpireLeader = true;
                _leaderSimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
                _empireGenerator.Verify(x => x.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[0]), Times.Once);
                _empireGenerator.Verify(x => x.CreateForSecedingLeader(It.IsAny<DeltaMetadata>(), _empire, _leaders[1]), Times.Once);
        }

        private void VerifyUpdate(Expression<Func<IEnumerable<Leader>, bool>> expression) =>
            _leaderRepository.Verify(x => x.SetLeadersForEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>(), It.Is(expression)), Times.Once);
    }
}