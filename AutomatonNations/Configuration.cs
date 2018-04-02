namespace AutomatonNations
{
    public class Configuration : IConfiguration
    {
        public DevelopmentCalculation DevelopmentCalculation
        {
            get => DevelopmentCalculation.SelfPriorityThenEqual;
        }

        public bool LimitGrowth
        {
            get => true;
        }

        public bool CapMilitaryProduction
        {
            get => true;
        }
    }
}