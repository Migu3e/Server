using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static List<Client> clients = new List<Client>();

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
            // receive the username from the client
            var buffer = new byte[1024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            var username = Encoding.UTF8.GetString(buffer, 0, received);

            // create a new Client instance with the handler and username
            Client temp = new Client(handler, username.Trim());
            clients.Add(temp);

            _ = Task.Run(() => HandleClient(temp));
        }
    }

    static async Task HandleClient(Client client)
    {
        Socket handler = client.client;


        while (true)
        {
            var buffer = new byte[1024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            if (received == 0)
            {
                break; // Client has disconnected
            }

            var receivedString = Encoding.UTF8.GetString(buffer, 0, received);
            var parts = receivedString.Split('|');
            if (parts.Length != 2)
            {
                continue; // Invalid message format
            }

            var username = parts[0];
            var message = parts[1];

            if (!string.IsNullOrEmpty(message))
            {
                if (message == "/Logout" || message == "/logout")
                {
                    await ServerPrivateMessage(client, "You Have Disconnected");
                    await ServerAllMessage(username, "Has Left The Chat", handler);
                    Console.WriteLine($"Server -  {username} has Disconected");
                    clients.Remove(client);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    break;
                }

                if (message == "/List" || message == "/list")
                {
                    string listOfOnlineClients = "The list of online clients are:";
                    foreach (var currClient in clients)
                    {
                       
                        if (currClient.username == client.username)
                        {
                            listOfOnlineClients += $"\n<--> {currClient.username} (you)";
                        }
                        else
                        {
                            listOfOnlineClients += $"\n<--> {currClient.username}";
                        }
                    }


                    await ServerPrivateMessage(client, listOfOnlineClients); // Ensure this is awaited
                    
                }
                else
                {
                    Console.WriteLine($"Message from {username}: {message}");
                    // Broadcast message to all clients except the sender
                    foreach (var c in clients)
                    {
                        if (c.client != handler)
                        {
                            var response = $"{username}: {message}";
                            var responseByte = Encoding.UTF8.GetBytes(response);
                            await c.client.SendAsync(responseByte, SocketFlags.None);
                        }
                    }
                }



                
            }
        }
        
        
    }

    static async Task ServerAllMessage(string username, string message, Socket handler)
    {
        var response = $"Server: {username} {message}";
        var responseByte = Encoding.UTF8.GetBytes(response);

        foreach (var client in clients)
        {
            if (client.client != handler)
            {
                await client.client.SendAsync(responseByte, SocketFlags.None);
            }
        }
    }

    static async Task ServerPrivateMessage(Client client, string message)
    {
        var response = $"Server: {message}";
        var responseByte = Encoding.UTF8.GetBytes(response);
        await client.client.SendAsync(responseByte, SocketFlags.None);
    }
}