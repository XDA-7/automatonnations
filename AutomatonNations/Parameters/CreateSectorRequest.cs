namespace AutomatonNations
{
    public class CreateSectorRequest
    {
        public Coordinate Coordinate { get; set; }

        public int Development { get; set; }

        public CreateSectorRequest(Coordinate coordinate, int development)
        {
            Coordinate = coordinate;
            Development = development;
        }
    }
}