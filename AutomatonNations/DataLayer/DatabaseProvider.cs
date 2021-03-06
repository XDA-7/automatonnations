using MongoDB.Driver;

namespace AutomatonNations
{
    public class DatabaseProvider : IDatabaseProvider
    {
        private IMongoClient _client;
        public IMongoDatabase Database { get; }

        public DatabaseProvider()
        {
            _client = new MongoClient();
            Database = _client.GetDatabase("AutomatonNations");
        }
    }
}