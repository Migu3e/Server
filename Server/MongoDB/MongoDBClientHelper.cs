using MongoDB.Driver;
using Server.Const;

namespace Server.MongoDB;

public class MongoDBClientHelper
{
    private static IMongoDatabase database;

    static MongoDBClientHelper()
    {
        // MongoDB connection string
        var client = new MongoClient(ConstMasseges.DataBaseConnection);
        // Database name
        database = client.GetDatabase("client");
    }

    public static IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return database.GetCollection<T>(collectionName);
    }
}