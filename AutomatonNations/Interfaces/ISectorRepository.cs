namespace AutomatonNations
{
    public interface ISectorRepository
    {
        StarSystem[] Create(Coordinate[] coordinates);

        void ConnectSystems(StarSystem[] starSystems);
    }
}