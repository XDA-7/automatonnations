namespace AutomatonNations
{
    public interface ISpatialOperations
    {
        Coordinate[] WithinRadius(Coordinate centreCoordinate, Coordinate[] coordinates, int radius);
    }
}