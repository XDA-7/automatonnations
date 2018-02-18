using System;
using MongoDB.Driver;

namespace AutomatonNations
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new DatabaseProvider().Database;
            var collection = database.GetCollection<StarSystem>("test");
            var system = new StarSystem { Development = 250 };
            collection.InsertOne(system);
            Console.WriteLine(system.Id);
            /*var builder = Builders<StarSystem>.Filter;
            var filter = builder.Where(x => true);
            var result = collection.FindSync(filter).ToList();
            Console.WriteLine(result[0].Id);
            Console.WriteLine(result[0].Development);*/
        }
    }
}
