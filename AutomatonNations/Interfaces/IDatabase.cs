using MongoDB.Driver;

namespace AutomatonNations
{
    public interface IDatabase
    {
        IMongoDatabase Database { get; }
    }
}