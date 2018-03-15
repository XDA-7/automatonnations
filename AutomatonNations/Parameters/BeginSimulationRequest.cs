namespace AutomatonNations
{
    public class BeginSimulationRequest
    {
        public BeginSimulationRequest(int sectorStarCount, int sectorSize, int systemConnectivityRadius)
        {
            SectorStarCount = sectorStarCount;
            SectorSize = sectorSize;
            SystemConnectivityRadius = systemConnectivityRadius;
        }

        public int SectorStarCount { get; set; }

        public int SectorSize { get; set; }

        public int SystemConnectivityRadius { get; set; }
    }
}