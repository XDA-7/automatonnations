using Moq;
using Xunit;

namespace AutomatonNations.Tests_MilitaryCalculator
{
    public class EmpireTotalMilitary
    {
        private Mock<IConfiguration> _configuration = new Mock<IConfiguration>();
        private Mock<IRandom> _random = new Mock<IRandom>();
        private IMilitaryCalculator _militaryCalculator;

        private Empire _empire;
        private Leader[] _leaders = new Leader[]
        {
            new Leader { Military = 250.0 },
            new Leader { Military = 100.0 },
            new Leader { Military = 50.0 }
        };

        public EmpireTotalMilitary()
        {
            _empire = new Empire
            {
                Military = 450.0,
                Leaders = _leaders
            };

            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
        }

        [Fact]
        public void ReturnsSumOfEmpireAndLeaderMilitaries()
        {
            var result = _militaryCalculator.EmpireTotalMilitary(_empire);
            Assert.Equal(850.0, result);
        }
    }
}