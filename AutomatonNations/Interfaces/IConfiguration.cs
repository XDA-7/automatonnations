namespace AutomatonNations
{
    public interface IConfiguration
    {
        DevelopmentCalculation DevelopmentCalculation { get; }

        bool LimitGrowth { get; } 

        bool CapMilitaryProduction { get; }
    }
}