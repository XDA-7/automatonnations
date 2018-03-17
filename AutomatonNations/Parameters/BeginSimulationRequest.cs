namespace AutomatonNations
{
    public class BeginSimulationRequest
    {
        public BeginSimulationRequest(int sectorStarCount, int sectorSize, int systemConnectivityRadius, int baseDevelopment)
        {
            SectorStarCount = sectorStarCount;
            SectorSize = sectorSize;
            SystemConnectivityRadius = systemConnectivityRadius;
            BaseDevelopment = baseDevelopment;
        }

        public int SectorStarCount { get; }

        public int SectorSize { get; }

        public int SystemConnectivityRadius { get; }

        public int BaseDevelopment { get; }
    }
}