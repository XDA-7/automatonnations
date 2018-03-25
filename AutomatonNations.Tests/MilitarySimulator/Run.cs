using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations
{
    public class Run
    {
        private Mock<IMilitaryCalculator> _militaryCalculator = new Mock<IMilitaryCalculator>();
        private Mock<IWarRepository> _warRepository = new Mock<IWarRepository>();
        private Mock<IEconomicSimulator> _economicSimulator = new Mock<IEconomicSimulator>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private IMilitarySimulator _militarySimulator;

        private ObjectId _attackerId = ObjectId.GenerateNewId();
        private ObjectId _attackerSystemId = ObjectId.GenerateNewId();
        private ObjectId _defenderId = ObjectId.GenerateNewId();
        private ObjectId _defenderSystemId = ObjectId.GenerateNewId();

        public Run()
        {
            _militarySimulator = new MilitarySimulator(
                _militaryCalculator.Object,
                _warRepository.Object,
                _economicSimulator.Object,
                _empireRepository.Object);

            _warRepository.Setup(x => x.GetWars(It.IsAny<ObjectId>()))
                .Returns(new War[] { new War { AttackerId = _attackerId, DefenderId = _defenderId } });

            var view = new EmpireBorderView
            {
                Empire = new Empire { Id = _attackerId },
                BorderingEmpire = new Empire { Id = _defenderId },
                EmpireSystems = new StarSystem[] { new StarSystem { Id = _attackerSystemId } },
                BorderingEmpireSystems = new StarSystem[] { new StarSystem { Id = _defenderSystemId } }
            };
            _empireRepository.Setup(x => x.GetEmpireBorderView(_attackerId, _defenderId))
                .Returns(view);
            
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<TerritoryGain>()));
        }

        [Fact]
        public void CalculatesNewOutcomeForEachWar()
        {
            _warRepository.Setup(x => x.GetWars(It.IsAny<ObjectId>()))
                .Returns(new War[]
                {
                    new War { AttackerId = _attackerId, DefenderId = _defenderId },
                    new War { AttackerId = _attackerId, DefenderId = _defenderId },
                    new War { AttackerId = _attackerId, DefenderId = _defenderId },
                    new War { AttackerId = _attackerId, DefenderId = _defenderId },
                    new War { AttackerId = _attackerId, DefenderId = _defenderId }
                });

            _militarySimulator.Run(It.IsAny<DeltaMetadata>(), ObjectId.GenerateNewId());

            _militaryCalculator.Verify(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()), Times.Exactly(5));
        }

        [Fact]
        public void AppliesCollateralDamageToBorderingSystems()
        {
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(
                    attackerMilitaryDamage: It.IsAny<double>(),
                    attackerCollateralDamage: 200.0,
                    defenderMilitaryDamage: It.IsAny<double>(),
                    defenderCollateralDamage: 300.0,
                    territoryGain: TerritoryGain.None));
            
            _militarySimulator.Run(It.IsAny<DeltaMetadata>(), ObjectId.GenerateNewId());

            _economicSimulator.Verify(x => x.ApplyDamage(
                It.IsAny<DeltaMetadata>(),
                It.IsAny<EmpireBorderView>(),
                200.0,
                300.0
            ), Times.Once);
        }

        [Fact]
        public void AppliesMilitaryDamageToOpposingEmpires()
        {
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(
                    attackerMilitaryDamage: 200.0,
                    attackerCollateralDamage: It.IsAny<double>(),
                    defenderMilitaryDamage: 300.0,
                    defenderCollateralDamage: It.IsAny<double>(),
                    territoryGain: TerritoryGain.None));
            
            _militarySimulator.Run(It.IsAny<DeltaMetadata>(), ObjectId.GenerateNewId());

            _empireRepository.Verify(x => x.ApplyMilitaryDamage(It.IsAny<DeltaMetadata>(), _attackerId, 300.0), Times.Once);
            _empireRepository.Verify(x => x.ApplyMilitaryDamage(It.IsAny<DeltaMetadata>(), _defenderId, 200.0), Times.Once);
        }

        [Fact]
        public void UpdatesWarWithMilitaryDamageInflicted()
        {
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(
                    attackerMilitaryDamage: 200.0,
                    attackerCollateralDamage: It.IsAny<double>(),
                    defenderMilitaryDamage: 300.0,
                    defenderCollateralDamage: It.IsAny<double>(),
                    territoryGain: TerritoryGain.None));

            _militarySimulator.Run(It.IsAny<DeltaMetadata>(), ObjectId.GenerateNewId());

            _warRepository.Verify(x => x.ContinueWar(It.IsAny<ObjectId>(), 200.0, 300.0));
        }

        [Fact]
        public void TransfersSystemsToAttackerIfTerritoryGain()
        {
            SetupSystemTransferTest(TerritoryGain.Attacker);

            _empireRepository.Verify(
                x => x.TransferSystems(
                    It.IsAny<DeltaMetadata>(),
                    _defenderId,
                    _attackerId,
                    It.Is<IEnumerable<ObjectId>>(y => y.Single() == _defenderSystemId)),
                Times.Once);
        }

        [Fact]
        public void TransfersSystemsToDefendersIfTerritoryGain()
        {
            SetupSystemTransferTest(TerritoryGain.Defender);
            _empireRepository.Verify(
                x => x.TransferSystems(
                    It.IsAny<DeltaMetadata>(),
                    _attackerId,
                    _defenderId,
                    It.Is<IEnumerable<ObjectId>>(y => y.Single() == _attackerSystemId)),
                Times.Once);
        }

        [Fact]
        public void DoesNotTransferSystemsIfNoTerritoryGain()
        {
            SetupSystemTransferTest(TerritoryGain.None);
            _empireRepository.Verify(
                x => x.TransferSystems(
                    It.IsAny<DeltaMetadata>(),
                    It.IsAny<ObjectId>(),
                    It.IsAny<ObjectId>(),
                    It.IsAny<IEnumerable<ObjectId>>()),
                Times.Never);
        }

        private void SetupSystemTransferTest(TerritoryGain territoryGain)
        {
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(
                    attackerMilitaryDamage: It.IsAny<double>(),
                    attackerCollateralDamage: It.IsAny<double>(),
                    defenderMilitaryDamage: It.IsAny<double>(),
                    defenderCollateralDamage: It.IsAny<double>(),
                    territoryGain: territoryGain));
                
            _militarySimulator.Run(It.IsAny<DeltaMetadata>(), ObjectId.GenerateNewId());
        }
    }
}