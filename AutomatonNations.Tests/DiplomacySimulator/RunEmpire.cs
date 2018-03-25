using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_DiplomacySimulator
{
    public class RunEmpire
    {
        private Mock<IDiplomacyCalculator> _diplomacyCalculator = new Mock<IDiplomacyCalculator>();
        private Mock<IWarRepository> _warRepository = new Mock<IWarRepository>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private IDiplomacySimulator _diplomacySimulator;

        private EmpireBorderView[] _borderViews = new EmpireBorderView[]
        {
            new EmpireBorderView { BorderingEmpire = new Empire { Id = ObjectId.GenerateNewId() } },
            new EmpireBorderView { BorderingEmpire = new Empire { Id = ObjectId.GenerateNewId() } },
            new EmpireBorderView { BorderingEmpire = new Empire { Id = ObjectId.GenerateNewId() } },
            new EmpireBorderView { BorderingEmpire = new Empire { Id = ObjectId.GenerateNewId() } },
            new EmpireBorderView { BorderingEmpire = new Empire { Id = ObjectId.GenerateNewId() } }
        };

        public RunEmpire()
        {
            _empireRepository.Setup(x => x.GetEmpireBorderViews(It.IsAny<ObjectId>()))
                .Returns(_borderViews);
            _diplomacySimulator = new DiplomacySimulator(_diplomacyCalculator.Object, _warRepository.Object, _empireRepository.Object);
        }

        [Fact]
        public void DoesNotDeclareWarIfAlreadyEngagedInOne()
        {
            _warRepository.Setup(x => x.GetWarsForEmpire(It.IsAny<ObjectId>()))
                .Returns(new War[] { new War() });
            _diplomacyCalculator.Setup(x => x.DeclareWar(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(true);
            
            _diplomacySimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());

            _warRepository.Verify(
                x => x.BeginWar(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>(), It.IsAny<ObjectId>()),
                Times.Never);
        }

        [Fact]
        public void DoesNotDeclareMoreThanOneWar()
        {
            _warRepository.Setup(x => x.GetWarsForEmpire(It.IsAny<ObjectId>()))
                .Returns(new War[0]);
            _diplomacyCalculator.Setup(x => x.DeclareWar(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(true);

            _diplomacySimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());

            _warRepository.Verify(
                x => x.BeginWar(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>(), It.IsAny<ObjectId>()),
                Times.Once);
        }

        [Fact]
        public void DeclaresWarWhenDeclareWarReturnsTrue()
        {
            var targetEmpire = _borderViews[2].BorderingEmpire;
            _warRepository.Setup(x => x.GetWarsForEmpire(It.IsAny<ObjectId>()))
                .Returns(new War[0]);
            _diplomacyCalculator.Setup(x => x.DeclareWar(It.IsAny<Empire>(), targetEmpire))
                .Returns(true);

            _diplomacySimulator.RunEmpire(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>());

            _warRepository.Verify(
                x => x.BeginWar(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>(), targetEmpire.Id),
                Times.Once);
        }
    }
}