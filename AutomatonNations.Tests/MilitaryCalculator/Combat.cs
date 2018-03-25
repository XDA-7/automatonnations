using Moq;
using Xunit;

namespace AutomatonNations.Tests_MilitaryCalculator
{
    public class Combat
    {
        private Mock<IRandom> _random = new Mock<IRandom>();
        private IMilitaryCalculator _militaryCalculator;

        public Combat()
        {
            _militaryCalculator = new MilitaryCalculator(_random.Object);
        }

        [Theory]
        [InlineData(0.0, 0.0, 0.0, 0.0)]
        [InlineData(300.0, 250.0, 0.0, 0.0)]
        [InlineData(0.0, 0.0, 0.1, 0.1)]
        [InlineData(300.0, 200.0, 0.1, 0.15)]
        [InlineData(250.0, 500.0, 0.15, 0.05)]
        public void DamageIsProductOfEmpireMilitaryAndRandomNumber(double attackerMilitary, double defenderMilitary, double attackerRandom, double defenderRandom)
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { attackerRandom, defenderRandom });
            var attacker = new Empire { Military = attackerMilitary };
            var defender = new Empire { Military = defenderMilitary };
            var expectedAttackerDamage = attackerMilitary * attackerRandom;
            var expectedDefenderDamage = defenderMilitary * defenderRandom;

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(expectedAttackerDamage, result.AttackerDamage.MilitaryDamage);
            Assert.Equal(expectedDefenderDamage, result.DefenderDamage.MilitaryDamage);
        }

        [Fact]
        public void CollateralDamageIsProportionalToMilitaryDamage()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            var attacker = new Empire { Military = 250.0 };
            var defender = new Empire { Military = 100.0 };

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(
                result.AttackerDamage.MilitaryDamage * Parameters.CollateralDamageRate,
                result.AttackerDamage.CollateralDamage);
            Assert.Equal(
                result.DefenderDamage.MilitaryDamage * Parameters.CollateralDamageRate,
                result.DefenderDamage.CollateralDamage);
        }

        [Fact]
        public void TerritoryGainToAttackerWhenSufficientAdvantage()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            var attacker = new Empire { Military = 610.0 };
            var defender = new Empire { Military = 100.0 };

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(TerritoryGain.Attacker, result.TerritoryGain);
        }

        [Fact]
        public void TerritoryGainToDefenderWhenSufficientAdvantage()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            var attacker = new Empire { Military = 100.0 };
            var defender = new Empire { Military = 610.0 };

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(TerritoryGain.Defender, result.TerritoryGain);
        }

        [Fact]
        public void NoTerritoryGainWhenAttackerAdvantageInsufficient()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            var attacker = new Empire { Military = 100.0 };
            var defender = new Empire { Military = 600.0 };

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(TerritoryGain.None, result.TerritoryGain);
        }

        [Fact]
        public void NoTerritoryGainWhenDefenderAdvantageInsufficient()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            var attacker = new Empire { Military = 600.0 };
            var defender = new Empire { Military = 100.0 };

            var result = _militaryCalculator.Combat(attacker, defender);

            Assert.Equal(TerritoryGain.None, result.TerritoryGain);
        }
    }
}