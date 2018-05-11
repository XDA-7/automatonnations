using System.Linq;
using Moq;
using Xunit;

namespace AutomatonNations.Tests_MilitaryCalculator
{
    public class EmpireMilitaryDamageDistribution
    {
        private Mock<IConfiguration> _configuration = new Mock<IConfiguration>();
        private Mock<IRandom> _random = new Mock<IRandom>();
        private IMilitaryCalculator _militaryCalculator;

        private Empire _empire;
        private Leader[] _leaders = new Leader[]
        {
            new Leader { Military = 250.0 },
            new Leader { Military = 100.0 },
            new Leader { Military = 50.0 },
            new Leader { Military = 150.0 }
        };

        public EmpireMilitaryDamageDistribution()
        {
            _empire = new Empire
            {
                Military = 450.0,
                Leaders = _leaders
            };

            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
        }

        [Fact]
        public void ReturnsEmpireMilitaryDamageProportionalToContribution()
        {
            var result = _militaryCalculator.EmpireMilitaryDamageDistribution(_empire, 200.0);
            Assert.Equal(90.0, result.EmpireDamage);
        }

        [Fact]
        public void ReturnsLeadersWithUpdatedMilitaryValues()
        {
            var result = _militaryCalculator.EmpireMilitaryDamageDistribution(_empire, 200.0);
            var leaderResults = result.UpdatedLeaders.ToArray();
            Assert.Equal(200.0, leaderResults[0].Military);
            Assert.Equal(80.0, leaderResults[1].Military);
            Assert.Equal(40.0, leaderResults[2].Military);
            Assert.Equal(120.0, leaderResults[3].Military);
        }
    }
}