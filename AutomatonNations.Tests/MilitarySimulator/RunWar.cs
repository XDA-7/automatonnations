using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace AutomatonNations
{
    public class RunWar
    {
        private Mock<IMilitaryCalculator> _militaryCalculator = new Mock<IMilitaryCalculator>();
        private Mock<IWarRepository> _warRepository = new Mock<IWarRepository>();
        private Mock<IEconomicSimulator> _economicSimulator = new Mock<IEconomicSimulator>();
        private Mock<IEmpireRepository> _empireRepository = new Mock<IEmpireRepository>();
        private Mock<ILeaderRepository> _leaderRepository = new Mock<ILeaderRepository>();
        private Mock<ISystemTransferrer> _systemTransferrer = new Mock<ISystemTransferrer>();
        private IMilitarySimulator _militarySimulator;

        private ObjectId _attackerId = ObjectId.GenerateNewId();
        private ObjectId _attackerSystemId = ObjectId.GenerateNewId();
        private ObjectId _defenderId = ObjectId.GenerateNewId();
        private ObjectId _defenderSystemId = ObjectId.GenerateNewId();
        private Leader[] _attackerLeaders = new Leader[0];
        private Leader[] _defenderLeaders = new Leader[0];


        public RunWar()
        {
            _militarySimulator = new MilitarySimulator(
                _militaryCalculator.Object,
                _warRepository.Object,
                _economicSimulator.Object,
                _empireRepository.Object,
                _leaderRepository.Object,
                _systemTransferrer.Object);

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

            _militaryCalculator.Setup(x => x.EmpireMilitaryDamageDistribution(It.IsAny<Empire>(), It.IsAny<double>()))
                .Returns(new EmpireMilitaryDamageResult(It.IsAny<double>(), new Leader[0]));
            
            _empireRepository.Setup(x => x.GetById(It.IsAny<ObjectId>()))
                .Returns(new Empire { StarSystemsIds = new ObjectId[0] });
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
            _militaryCalculator.Setup(x => x.EmpireMilitaryDamageDistribution(It.IsAny<Empire>(), 200.0))
                .Returns(new EmpireMilitaryDamageResult(200.0, _attackerLeaders));
            _militaryCalculator.Setup(x => x.EmpireMilitaryDamageDistribution(It.IsAny<Empire>(), 300.0))
                .Returns(new EmpireMilitaryDamageResult(200.0, _defenderLeaders));
            
            _militarySimulator.Run(It.IsAny<DeltaMetadata>(), ObjectId.GenerateNewId());

            _empireRepository.Verify(x => x.ApplyMilitaryDamage(It.IsAny<DeltaMetadata>(), _attackerId, 300.0), Times.Once);
            _empireRepository.Verify(x => x.ApplyMilitaryDamage(It.IsAny<DeltaMetadata>(), _defenderId, 200.0), Times.Once);
        }

        [Fact]
        public void UpdatesEmpireLeadersWithResultOfDamageDistribution()
        {
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(
                    attackerMilitaryDamage: It.IsAny<double>(),
                    attackerCollateralDamage: It.IsAny<double>(),
                    defenderMilitaryDamage: It.IsAny<double>(),
                    defenderCollateralDamage: It.IsAny<double>(),
                    territoryGain: TerritoryGain.None));
            _militaryCalculator.Setup(x => x.EmpireMilitaryDamageDistribution(It.IsAny<Empire>(), 200.0))
                .Returns(new EmpireMilitaryDamageResult(It.IsAny<double>(), _attackerLeaders));
            _militaryCalculator.Setup(x => x.EmpireMilitaryDamageDistribution(It.IsAny<Empire>(), 300.0))
                .Returns(new EmpireMilitaryDamageResult(It.IsAny<double>(), _defenderLeaders));

            _militarySimulator.Run(It.IsAny<DeltaMetadata>(), ObjectId.GenerateNewId());

            _leaderRepository.Verify(x => x.SetLeadersForEmpire(It.IsAny<DeltaMetadata>(), _attackerId, _attackerLeaders), Times.Once);
            _leaderRepository.Verify(x => x.SetLeadersForEmpire(It.IsAny<DeltaMetadata>(), _defenderId, _defenderLeaders), Times.Once);
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

            _warRepository.Verify(x => x.ContinueWar(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>(), 200.0, 300.0));
        }

        [Fact]
        public void TransfersSystemsToAttackerIfTerritoryGain()
        {
            SetupSystemTransferTest(TerritoryGain.Attacker);

            _systemTransferrer.Verify(
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
            _systemTransferrer.Verify(
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
            _systemTransferrer.Verify(
                x => x.TransferSystems(
                    It.IsAny<DeltaMetadata>(),
                    It.IsAny<ObjectId>(),
                    It.IsAny<ObjectId>(),
                    It.IsAny<IEnumerable<ObjectId>>()),
                Times.Never);
        }

        [Fact]
        public void EndsWarIfAttackerLosesAllTerritory()
        {
            _empireRepository.Setup(x => x.GetById(_attackerId))
                .Returns(new Empire { StarSystemsIds = new ObjectId[0] });

            SetupSystemTransferTest(TerritoryGain.Defender);

            _warRepository.Verify(x => x.EndWarsWithParticipant(It.IsAny<DeltaMetadata>(), _attackerId), Times.Once);
        }

        [Fact]
        public void DoesntEndWarIfAttackerDoesntLoseAllTerritory()
        {
            _empireRepository.Setup(x => x.GetById(_attackerId))
                .Returns(new Empire { StarSystemsIds = new ObjectId[] { ObjectId.GenerateNewId() } });

            SetupSystemTransferTest(TerritoryGain.Defender);

            _warRepository.Verify(x => x.EndWarsWithParticipant(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>()), Times.Never);
        }

        [Fact]
        public void EndsWarIfDefenderLosesAllTerritory()
        {
            _empireRepository.Setup(x => x.GetById(_defenderId))
                .Returns(new Empire { StarSystemsIds = new ObjectId[0] });

            SetupSystemTransferTest(TerritoryGain.Attacker);

            _warRepository.Verify(x => x.EndWarsWithParticipant(It.IsAny<DeltaMetadata>(), _defenderId), Times.Once);
        }

        [Fact]
        public void DoesntEndWarIfDefenderDoesntLoseAllTerritory()
        {
            _empireRepository.Setup(x => x.GetById(_defenderId))
                .Returns(new Empire { StarSystemsIds = new ObjectId[] { ObjectId.GenerateNewId() } });

            SetupSystemTransferTest(TerritoryGain.Attacker);

            _warRepository.Verify(x => x.EndWarsWithParticipant(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>()), Times.Never);
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