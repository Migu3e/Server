// File: Server/MongoDB/MongoDBHelper.cs
using MongoDB.Driver;
using Server.Const;

namespace Server.MongoDB
{
    public static class MongoDBHelper
    {
        private static IMongoDatabase database;

        static MongoDBHelper()
        {
            var client = new MongoClient(ConstMasseges.DataBaseConnection);
            database = client.GetDatabase("chats");
        }
        

        public static IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return database.GetCollection<T>(collectionName);
        }
    }
}