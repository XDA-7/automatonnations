using Moq;
using Xunit;

namespace AutomatonNations.Tests_MilitaryCalculator
{
    public class Combat
    {
        private Mock<IRandom> _random = new Mock<IRandom>();
        private IMilitaryCalculator _militaryCalculator;

        [Theory]
        [InlineData(0, 0, 0.0, 0.0, 0, 0)]
        [InlineData(300, 250, 0.0, 0.0, 0, 0)]
        [InlineData(0, 0, 0.1, 0.1, 0, 0)]
        [InlineData(300, 200, 0.1, 0.15, 30, 30)]
        [InlineData(250, 500, 0.15, 0.05, 37, 25)]
        public void DamageIsProductOfEmpireMilitaryAndRandomNumber(int attackerMilitary, int defenderMilitary, double attackerRandom, double defenderRandom, int expectedAttackerDamage, int expectedDefenderDamage)
        {
            _random.Setup(x => x.DoubleSet(2))
                .Returns(new double[] { attackerRandom, defenderRandom });
            var attacker = new Empire { Military = attackerMilitary };
            var defender = new Empire { Military = defenderMilitary };

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(expectedAttackerDamage, result.AttackerDamage.MilitaryDamage);
            Assert.Equal(expectedDefenderDamage, result.DefenderDamage.MilitaryDamage);
        }

        [Fact]
        public void TerritoryGainToAttackerWhenSufficientAdvantage()
        {
            _random.Setup(x => x.DoubleSet(2))
                .Returns(new double[] { 0.1, 0.1 });
            var attacker = new Empire { Military = 100.0 };
            var defender = new Empire { Military = 610.0 };

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(TerritoryGain.Attacker, result.TerritoryGain);
        }

        [Fact]
        public void AttackerCollateralDamageIsProportionalToMilitaryDamage()
        {
            _random.Setup(x => x.DoubleSet(2))
                .Returns(new double[] { 0.1, 0.1 });
        }

        [Fact]
        public void DefenderCollateralDamageIsProportionalToMilitaryDamage()
        {
        }

        [Fact]
        public void TerritoryGainToDefenderWhenSufficientAdvantage()
        {
        }

        [Fact]
        public void NoTerritoryGainWhenAttackerAdvantageInsufficient()
        {}

        [Fact]
        public void NoTerritoryGainWhenDefenderAdvantageInsufficient()
        {}
    }
}