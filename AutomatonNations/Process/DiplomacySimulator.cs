using System.Linq;
using MongoDB.Bson;

namespace AutomatonNations
{
    public class DiplomacySimulator : IDiplomacySimulator
    {
        private IDiplomacyCalculator _diplomacyCalculator;
        private IWarRepository _warRepository;
        private IEmpireRepository _empireRepository;
        
        public DiplomacySimulator(IDiplomacyCalculator diplomacyCalculator, IWarRepository warRepository, IEmpireRepository empireRepository)
        {
            _diplomacyCalculator = diplomacyCalculator;
            _warRepository = warRepository;
            _empireRepository = empireRepository;
        }

        public void RunEmpire(DeltaMetadata deltaMetadata, ObjectId empireId)
        {
            var existingWars = _warRepository.GetWarsForEmpire(empireId);
            if (existingWars.Any())
            {
                return;
            }

            var borderingEmpires = _empireRepository.GetEmpireBorderViews(empireId);
            foreach (var borderEmpire in borderingEmpires)
            {
                if (_diplomacyCalculator.DeclareWar(borderEmpire.Empire, borderEmpire.BorderingEmpire))
                {
                    _warRepository.BeginWar(deltaMetadata, empireId, borderEmpire.BorderingEmpire.Id);
                    break;
                }
            }
        }
    }
}