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

        public int SectorStarCount { get; set; }

        public int SectorSize { get; set; }

        public int SystemConnectivityRadius { get; set; }

        public int BaseDevelopment { get; set; }
    }
}