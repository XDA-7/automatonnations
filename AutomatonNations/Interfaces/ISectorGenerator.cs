using MongoDB.Bson;

namespace AutomatonNations
{
    public interface ISectorGenerator
    {
        CreateSectorResult CreateSector(int starCount, int size, int connectivityRadius);
    }
}