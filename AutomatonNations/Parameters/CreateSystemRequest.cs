namespace AutomatonNations
{
    public class CreateSystemRequest
    {
        public Coordinate Coordinate { get; }

        public int Development { get; }

        public CreateSystemRequest(Coordinate coordinate, int development)
        {
            Coordinate = coordinate;
            Development = development;
        }
    }
}