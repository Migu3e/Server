// File: Server/MongoDB/MongoDBHelper.cs
using MongoDB.Driver;

namespace Server.MongoDB
{
    public static class MongoDBHelper
    {
        private static IMongoDatabase database;

        static MongoDBHelper()
        {
            var client = new MongoClient("mongodb+srv://pc:123123gg123123@cluster0.tjadqzu.mongodb.net/");
            database = client.GetDatabase("chats");
        }

        public static IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return database.GetCollection<T>(collectionName);
        }
    }
}