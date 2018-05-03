namespace AutomatonNations
{
    public static class Parameters
    {
        public const double IncomeRate = 0.02;

        public const double IncomeReservedForSelf = 0.5;

        public const double MilitaryDamageRateMinimum = 0.0;

        public const double MilitaryDamageRateMaximum = 0.2;

        // Military advantage is (damage - enemyDamage) / enemyForce
        public const double MilitaryAdvantageLineAdvanceThreshold = 0.5;

        public const double CollateralDamageRate = 0.05;

        public const double MilitaryCapDevelopmentProportion = 20.0;

        public const double LeaderCreationChancePerSystemPerTick = 0.1;

        public const int LeaderInitialSystemLimit = 3;

        public const int LeaderMaxSystemLimit = 20;

        public const double LeaderSystemLimitIncreaseChancePerTurn = 0.25;

        public const double LeaderMinimumDevelopmentBonus = 0.5;

        public const double LeaderMaximumDevelopmentBonus = 3.0;

        public const double LeaderMinimumMilitaryWitholdingRate = 0.2;

        public const double LeaderMaximumMilitaryWitholdingRate = 0.8;
    }
}