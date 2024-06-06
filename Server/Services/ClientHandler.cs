using System.Net.Sockets;
using System.Text;
using Server.Interfaces;


namespace Server.Services;

public class ClientHandler : ICleintHandler
{
    public ClientHandler(IChatServer chatServer, IRoomServices roomServices, IPrivateChatHandler privateChatHandler)
    {
        _chatServer = chatServer;
        _roomServices = roomServices;
        _privateChatHandler = privateChatHandler;
    }

    private IChatServer _chatServer;
    private IRoomServices _roomServices;
    private IPrivateChatHandler _privateChatHandler;
    

    public async Task HandleClient(IClient client)
    {
        Socket handler = client.ClientSocket;
        while (true)
        {
            var buffer = new byte[1024];
            var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
            if (received == 0)
            {
                break;
            }

            var receivedString = Encoding.UTF8.GetString(buffer, 0, received);
            var parts = receivedString.Split('|');
            if (parts.Length < 2)
            {
                continue;
            }

            var username = parts[0];
            var message = parts[1];
            if (!string.IsNullOrEmpty(message))
            {
                switch (message.ToLower())
                {
                    case "/help":
                        await _chatServer.ServerPrivateMessage(client, "The commands are:\n" +
                                                                      "/list - Display the connected users\n" +
                                                                      "/logout - Disconnect from the server\n" +
                                                                      "/croom <roomname> - Create a new room with the specified name\n" +
                                                                      "/jroom <roomname> - Join an existing room with the specified name\n" +
                                                                      "/iroom <username> <roomname> - Invite a user to a specified room\n" +
                                                                      "/leave - Leave the current room and return to the main room\n" +
                                                                      "/list rooms - List all rooms and their members\n" +
                                                                      "/private <username> enters the private chat with the user selected\n" +
                                                                      "/help - Display this list of commands");
                        return;
                    case "/logout":
                        await HandleLogout(client, username);
                        return;

                    case "/list":
                        await SendClientList(client);
                        break;

                    default:
                        if (message.StartsWith("/croom"))
                        {
                            await _roomServices.HandleCreateRoom(client, message);
                        }
                        else if (message.StartsWith("/jroom"))
                        {
                            await _roomServices.HandleJoinRoom(client, message);
                        }
                        else if (message.StartsWith("/iroom"))
                        {
                            await _roomServices.HandleInviteRoom(client, message);
                        }
                        else if (message.StartsWith("/leave"))
                        {
                            await _roomServices.LeaveRoom(client);
                        }
                        else if (message.StartsWith("/list rooms"))
                        {
                            await _roomServices.PrintRooms(client);
                        }
                        else if (message.StartsWith("/private"))
                        {
                            await _privateChatHandler.HandleJoinPrivateRoom(client, message);
                        }
                        else
                        {
                            await _roomServices.SendMessageToRoom(client.Username,message,client.RoomName);
                                
                        }
                        break;
                }
            }
        }
    }

    public async Task HandleLogout(IClient client, string username)
    {
        await _chatServer.ServerPrivateMessage(client, "You have disconnected");
        await _roomServices.SendMessageToRoom(username, "has left the chat", client.RoomName);
        Console.WriteLine($"Server - {username} has disconnected");
        _chatServer.clients.Remove(client);
        await _roomServices.LeaveRoom(client);
        client.ClientSocket.Shutdown(SocketShutdown.Both);
        client.ClientSocket.Close();
    }

    public async Task SendClientList(IClient client)
    {
        string listOfOnlineClients = "The list of online clients are:";
        foreach (var currClient in _chatServer.clients)
        {
            listOfOnlineClients += $"\n<--> {currClient.Username}" + (currClient.Username == client.Username ? " (you)" : "");
        }
        await _chatServer.ServerPrivateMessage(client, listOfOnlineClients);
    }

    public async Task UpdatedClientList(IClient client)
    {
        string listOfOnlineClients = "The list of online clients are has changed\n";
        foreach (var currClient in _chatServer.clients)
        {
            listOfOnlineClients += $"<--> {currClient.Username}" + (currClient.Username == client.Username ? " (Just Joined)\n" : "\n");
        }
        await _chatServer.PrintToAll(client, listOfOnlineClients);
    }

}
