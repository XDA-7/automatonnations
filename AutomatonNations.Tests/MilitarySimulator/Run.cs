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
        private ObjectId _defenderId = ObjectId.GenerateNewId();

        public Run()
        {
            _warRepository.Setup(x => x.GetWars(It.IsAny<ObjectId>()))
                .Returns(new War[]
                {
                    new War
                    {
                        AttackerId = _attackerId,
                        DefenderId = _defenderId
                    }
                });
        }

        [Fact]
        public void CalculatesNewOutcomeForEachWar()
        {
            _militarySimulator.Run(new Simulation
            {
                WarIds = new ObjectId[]
                {
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId(),
                    ObjectId.GenerateNewId()
                }
            });

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
            
            _militarySimulator.Run(new Simulation
            {
                WarIds = new ObjectId[] { ObjectId.GenerateNewId() }
            });

            _economicSimulator.Verify(x => x.ApplyDamage(
                It.IsAny<DeltaMetadata>(),
                It.IsAny<EmpireBorderView>(),
                200.0,
                300.0
            ), Times.Once);
        }

        [Fact]
        public void AppliesMilitaryDamageToEmpires()
        {
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(
                    attackerMilitaryDamage: 200.0,
                    attackerCollateralDamage: It.IsAny<double>(),
                    defenderMilitaryDamage: 300.0,
                    defenderCollateralDamage: It.IsAny<double>(),
                    territoryGain: TerritoryGain.None));
            
            _militarySimulator.Run(new Simulation
            {
                WarIds = new ObjectId[] { ObjectId.GenerateNewId() }
            });

            _empireRepository.Verify(x => x.ApplyMilitaryDamage(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>(), 300.0), Times.Once);
            _empireRepository.Verify(x => x.ApplyMilitaryDamage(It.IsAny<DeltaMetadata>(), It.IsAny<ObjectId>(), 200.0), Times.Once);
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

            _militarySimulator.Run(new Simulation
            {
                WarIds = new ObjectId[] { ObjectId.GenerateNewId() }
            });

            _warRepository.Verify(x => x.ContinueWar(It.IsAny<ObjectId>(), 200.0, 300.0));
        }

        [Fact]
        public void TransfersSystemsToAttackerIfTerritoryGain()
        {
            _militaryCalculator.Setup(x => x.Combat(It.IsAny<Empire>(), It.IsAny<Empire>()))
                .Returns(new CombatResult(
                    attackerMilitaryDamage: 200.0,
                    attackerCollateralDamage: It.IsAny<double>(),
                    defenderMilitaryDamage: 300.0,
                    defenderCollateralDamage: It.IsAny<double>(),
                    territoryGain: TerritoryGain.Attacker));
                
            _militarySimulator.Run(new Simulation
            {
                WarIds = new ObjectId[] { ObjectId.GenerateNewId() }
            });

            _empireRepository.Verify(x => x.TransferSystems())
        }

        [Fact]
        public void TransfersSystemsToDefendersIfTerritoryGain()
        {}

        [Fact]
        public void DoesNotTransferSystemsIfNoTerritoryGain()
        {}
    }
}