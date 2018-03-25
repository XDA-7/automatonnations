namespace AutomatonNations
{
    public class DiplomacyCalculator : IDiplomacyCalculator
    {
        private IRandom _random;

        public DiplomacyCalculator(IRandom random)
        {
            _random = random;
        }

        public bool DeclareWar(Empire belligerent, Empire target)
        {
            var targetRelativeSize = (target.Military / belligerent.Military);
            var random = _random.DoubleSet(0.0, 1.0, 1)[0];
            return random >= targetRelativeSize;
        }
    }
}