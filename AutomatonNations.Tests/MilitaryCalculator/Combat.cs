using Moq;
using Xunit;

namespace AutomatonNations.Tests_MilitaryCalculator
{
    public class Combat
    {
        private Mock<IConfiguration> _configuration = new Mock<IConfiguration>();
        private Mock<IRandom> _random = new Mock<IRandom>();
        private IMilitaryCalculator _militaryCalculator;

        private Empire _attacker = new Empire { Leaders = new Leader[0] };
        private Empire _defender = new Empire { Leaders = new Leader[0] };

        public Combat()
        {
            _militaryCalculator = new MilitaryCalculator(_configuration.Object, _random.Object);
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
            _attacker.Military = attackerMilitary;
            _defender.Military = defenderMilitary;
            var expectedAttackerDamage = attackerMilitary * attackerRandom;
            var expectedDefenderDamage = defenderMilitary * defenderRandom;

            var result = _militaryCalculator.Combat(_attacker, _defender);

            Assert.Equal(expectedAttackerDamage, result.AttackerDamage.MilitaryDamage);
            Assert.Equal(expectedDefenderDamage, result.DefenderDamage.MilitaryDamage);
        }

        [Fact]
        public void EmpireMilitaryIncludesLeaderMilitaries()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            _attacker.Military = 250.0;
            _attacker.Leaders = new Leader[]
            {
                new Leader { Military = 300.0 },
                new Leader { Military = 100.0 }
            };
            _defender.Military = 100.0;
            _defender.Leaders = new Leader[]
            {
                new Leader { Military = 400.0 },
                new Leader { Military = 100.0 },
                new Leader { Military = 1500.0 }
            };

            var result = _militaryCalculator.Combat(_attacker, _defender);

            Assert.Equal(65.0, result.AttackerDamage.MilitaryDamage);
            Assert.Equal(210.0, result.DefenderDamage.MilitaryDamage);
        }

        [Fact]
        public void CollateralDamageIsProportionalToMilitaryDamage()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            _attacker.Military = 250.0;
            _defender.Military = 100.0;

            var result = _militaryCalculator.Combat(_attacker, _defender);

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
            _attacker.Military = 610.0;
            _defender.Military = 100.0;

            var result = _militaryCalculator.Combat(_attacker, _defender);

            Assert.Equal(TerritoryGain.Attacker, result.TerritoryGain);
        }

        [Fact]
        public void TerritoryGainToDefenderWhenSufficientAdvantage()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            _defender.Military = 610.0;
            _attacker.Military = 100.0;

            var result = _militaryCalculator.Combat(_attacker, _defender);

            Assert.Equal(TerritoryGain.Defender, result.TerritoryGain);
        }

        [Fact]
        public void NoTerritoryGainWhenAttackerAdvantageInsufficient()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            _attacker.Military = 100.0;
            _defender.Military = 600.0;

            var result = _militaryCalculator.Combat(_attacker, _defender);

            Assert.Equal(TerritoryGain.None, result.TerritoryGain);
        }

        [Fact]
        public void NoTerritoryGainWhenDefenderAdvantageInsufficient()
        {
            _random.Setup(x => x.DoubleSet(Parameters.MilitaryDamageRateMinimum, Parameters.MilitaryDamageRateMaximum, 2))
                .Returns(new double[] { 0.1, 0.1 });
            _attacker.Military = 600.0;
            _defender.Military = 100.0;

            var result = _militaryCalculator.Combat(_attacker, _defender);

            Assert.Equal(TerritoryGain.None, result.TerritoryGain);
        }
    }
}