using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_LeaderGenerator
{
    public class GenerateLeadersForEmpire
    {
        private Mock<ILeaderRepository> _leaderRepository = new Mock<ILeaderRepository>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private Mock<IRandom> _random = new Mock<IRandom>();
        private ILeaderGenerator _leaderGenerator;

        private Empire _empire = new Empire
        {
            StarSystemsIds = new ObjectId[]
            {
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId(),
                ObjectId.GenerateNewId()
            }
        };

        public GenerateLeadersForEmpire()
        {
            _leaderGenerator = new LeaderGenerator(_leaderRepository.Object, _empireRepository.Object, _random.Object);
            _empireRepository
                .Setup(x => x.GetById(It.IsAny<ObjectId>()))
                .Returns(_empire);
            _random
                .Setup(x => x.DoubleSet(It.IsAny<double>(), It.IsAny<double>(), 2))
                .Returns(new double[] { 0.0, 0.0 });
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 6))
                .Returns(new double[] { 0.43, 0.06, 0.73, 0.31, 0.03, 0.92 });
        }

        [Fact]
        public void CalculatesLeaderGenerationChancePerSystem()
        {
            _leaderGenerator.GenerateLeadersForEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            _random.Verify(x => x.DoubleSet(0.0, 1.0, 6), Times.Once);
        }

        [Fact]
        public void CreatesLeaderPerAcceptedRandomNumber()
        {
            _leaderGenerator.GenerateLeadersForEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            _leaderRepository.Verify(
                x => x.SetLeadersForEmpire(
                    It.IsAny<DeltaMetadata>(),
                    It.IsAny<ObjectId>(),
                    It.Is<IEnumerable<Leader>>(leaders => leaders.Count() == 2)),
                Times.Once);
        }

        [Fact]
        public void CreatesLeaderWithInitialSystemLimit()
        {
            SetupSingleLeaderCreate();
            _leaderGenerator.GenerateLeadersForEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            _leaderRepository.Verify(
                x => x.SetLeadersForEmpire(
                    It.IsAny<DeltaMetadata>(),
                    It.IsAny<ObjectId>(),
                    It.Is<IEnumerable<Leader>>(leaders => leaders.Single().SystemLimit == Parameters.LeaderInitialSystemLimit)),
                Times.Once);
        }

        [Fact]
        public void CreatesLeaderWtihDevelopmentBonusCorrespondingToRandomNumber()
        {
            SetupSingleLeaderCreate();
            _random
                .Setup(x => x.DoubleSet(
                    Parameters.LeaderMinimumDevelopmentBonus,
                    Parameters.LeaderMaximumDevelopmentBonus,
                    1))
                .Returns(new double[] { 2.3 });
            _leaderGenerator.GenerateLeadersForEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            _leaderRepository.Verify(
                x => x.SetLeadersForEmpire(
                    It.IsAny<DeltaMetadata>(),
                    It.IsAny<ObjectId>(),
                    It.Is<IEnumerable<Leader>>(leaders => leaders.Single().IncomeRateBonus == 2.3)),
                Times.Once);
        }

        [Fact]
        public void CreatesLeaderWithMilitaryWitholdingRateCorrespondingToRandomNumber()
        {
            SetupSingleLeaderCreate();
            _random
                .Setup(x => x.DoubleSet(
                    Parameters.LeaderMinimumMilitaryWitholdingRate,
                    Parameters.LeaderMaximumMilitaryWitholdingRate,
                    1))
                .Returns(new double[] { 0.52 });
            _leaderGenerator.GenerateLeadersForEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());
            _leaderRepository.Verify(
                x => x.SetLeadersForEmpire(
                    It.IsAny<DeltaMetadata>(),
                    It.IsAny<ObjectId>(),
                    It.Is<IEnumerable<Leader>>(leaders => leaders.Single().MilitaryWitholdingRate == 0.52)),
                Times.Once);
        }

        private void SetupSingleLeaderCreate()
        {
            _empire.StarSystemsIds = new ObjectId[] { ObjectId.GenerateNewId() };
            _random
                .Setup(x => x.DoubleSet(0.0, 1.0, 1))
                .Returns(new double[] { 0.0 });
            _random
                .Setup(x => x.DoubleSet(
                    Parameters.LeaderMinimumDevelopmentBonus,
                    Parameters.LeaderMaximumDevelopmentBonus,
                    1))
                .Returns(new double[] { 0.0 });
            _random
                .Setup(x => x.DoubleSet(
                    Parameters.LeaderMinimumMilitaryWitholdingRate,
                    Parameters.LeaderMaximumMilitaryWitholdingRate,
                    1))
                .Returns(new double[] { 0.0 });
        }
    }
}