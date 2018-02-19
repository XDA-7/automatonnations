using MongoDB.Driver;

namespace AutomatonNations
{
    public interface IDatabaseProvider
    {
        IMongoDatabase Database { get; }
    }
}