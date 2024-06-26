using MongoDB.Driver;
using Server.Const;

namespace Server.MongoDB;

public class MongoDBClientHelper
{
    private static IMongoDatabase database;

    static MongoDBClientHelper()
    {
        // MongoDB connection string
        var client = new MongoClient(ConstMasseges.DatabaseConnection);
        // Database name
        database = client.GetDatabase(ConstMasseges.DatabaseName);
    }

    public static IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return database.GetCollection<T>(collectionName);
    }
}