using Server.Services;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var chatServer = new ChatServer();
            await chatServer.StartAsync();
        }
    }
}