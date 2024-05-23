using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static List<Socket> clients = new List<Socket>();

    static async Task Main(string[] args)
    {
        IPHostEntry ipEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());
        IPAddress ip = ipEntry.AddressList[0];
        IPEndPoint ipEndPoint = new(ip, 1234);

        using Socket server = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(ipEndPoint);
        server.Listen();
        Console.WriteLine("Server has started listening on port 1234");

        while (true)
        {
            Socket handler = await server.AcceptAsync();
            clients.Add(handler);
            _ = Task.Run(() => HandleClient(handler));
        }
    }

    static async Task HandleClient(Socket handler)
    {
        while (true)
        {
            var buffer = new byte[1024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var receivedString = Encoding.UTF8.GetString(buffer, 0, received);
            var parts = receivedString.Split('|');
            var username = parts[0];
            var message = parts[1];

            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"Message from {username}: {message}");
                
                // Broadcast message to all clients except the sender
                foreach (var client in clients)
                {
                    if (client != handler)
                    {
                        var response = $"{username}: {message}";
                        var responseByte = Encoding.UTF8.GetBytes(response);
                        await client.SendAsync(responseByte, SocketFlags.None);
                    }
                }
            }
        }
    }
}