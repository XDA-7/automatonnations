using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_EmpireGenerator
{
    public class CreatePerSystem
    {
        private Mock<IRandom> _random = new Mock<IRandom>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private IEmpireGenerator _empireGenerator;

        public CreatePerSystem()
        {
            _empireGenerator = new EmpireGenerator(_random.Object, _empireRepository.Object);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(15)]
        public void CreatesEmpireForEachSystem(int systemCount)
        {
            _random.Setup(x => x.IntegerSet(It.IsAny<int>(), systemCount))
                .Returns(new int[systemCount]);
            var systemIds = new ObjectId[systemCount];
            for (var i = 0; i < systemCount; i++)
            {
                systemIds[i] = ObjectId.GenerateNewId();
            }

            var result = _empireGenerator.CreatePerSystem(systemCount, systemIds);

            _empireRepository.Verify(x => x.Create(It.Is<IEnumerable<CreateEmpireRequest>>(y => IsOneEmpirePerSystem(y, systemIds))), Times.Once);
        }

        private bool IsOneEmpirePerSystem(IEnumerable<CreateEmpireRequest> requests, IEnumerable<ObjectId> systemIds)
        {
            if (requests.Count() != systemIds.Count())
            {
                return false;
            }

            foreach (var request in requests)
            {
                if (request.StarSystemIds.Count() != 1)
                {
                    return false;
                }

                if (!systemIds.Contains(request.StarSystemIds.Single()))
                {
                    return false;
                }
            }

            return true;
        }

        [Fact]
        public void CreatesRandomizedAlignmentForEachSystem()
        {
            _random.Setup(x => x.IntegerSet(100, 5))
                .Returns(new int[] { 12, 54, 47, 88, 14 });
            
            var result = _empireGenerator.CreatePerSystem(5, new ObjectId[5]);

            _empireRepository.Verify(x => x.Create(It.Is<IEnumerable<CreateEmpireRequest>>(y =>
                y.ToArray()[0].Alignment.Prosperity == 0.12 &&
                y.ToArray()[1].Alignment.Prosperity == 0.54 &&
                y.ToArray()[2].Alignment.Prosperity == 0.47 &&
                y.ToArray()[3].Alignment.Prosperity == 0.88 &&
                y.ToArray()[4].Alignment.Prosperity == 0.14)),
            Times.Once);
        }
    }
}