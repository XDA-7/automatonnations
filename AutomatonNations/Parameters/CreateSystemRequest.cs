namespace AutomatonNations
{
    public class CreateSystemRequest
    {
        public Coordinate Coordinate { get; set; }

        public int Development { get; set; }

        public CreateSystemRequest(Coordinate coordinate, int development)
        {
            Coordinate = coordinate;
            Development = development;
        }
    }
}