using System.Net;
using System.Net.Sockets;
using System.Text;
using Server.Interfaces;
using Server.Models;

namespace Server.Services
{
    public class ChatServer : IChatServer
    {
        private List<IClient> clients = new List<IClient>();
        private List<IRoom> rooms = new List<IRoom>();

        public async Task StartAsync()
        {
            IPHostEntry ipEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());
            IPAddress ip = ipEntry.AddressList[1];
            Console.WriteLine($"IP is {ip}");
            IPEndPoint ipEndPoint = new(ip, 1234);

            using Socket server = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipEndPoint);
            server.Listen();
            Console.WriteLine("Server has started listening on port 1234");

            IRoom defaultRoom = new Room("Main");
            rooms.Add(defaultRoom);
            


            while (true)
            {
                Socket handler = await server.AcceptAsync();
                var buffer = new byte[1024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var username = Encoding.UTF8.GetString(buffer, 0, received).Trim();

                IClient client = new Client(handler, username, defaultRoom.Name);
                defaultRoom.AddClientToRoom(client);
                clients.Add(client);

                Console.WriteLine($"The client {client.Username} has connected to the server");
                await SendMessageToRoom("Server", $"{client.Username} has joined the room {client.RoomName}", client.RoomName);
                _ = Task.Run(() => HandleClient(client));
                
            }
        }

        private async Task HandleClient(IClient client)
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
                            await SendPrivateMessage(client, "The commands are:\n" +
                                                             "/list - Display the connected users\n" +
                                                             "/logout - Disconnect from the server\n" +
                                                             "/croom <roomname> - Create a new room with the specified name\n" +
                                                             "/jroom <roomname> - Join an existing room with the specified name\n" +
                                                             "/iroom <username> <roomname> - Invite a user to a specified room\n" +
                                                             "/leave - Leave the current room and return to the main room\n" +
                                                             "/list rooms - List all rooms and their members\n" +
                                                             "/whisper <username> <message> - Send a private message to a specific user\n" +
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
                                await HandleCreateRoom(client, message);
                            }
                            else if (message.StartsWith("/jroom"))
                            {
                                await HandleJoinRoom(client, message);
                            }
                            else if (message.StartsWith("/iroom"))
                            {
                                await HandleInviteRoom(client, message);
                            }
                            else if (message.StartsWith("/leave"))
                            {
                                await LeaveRoom(client);
                            }
                            else if (message.StartsWith("/list rooms"))
                            {
                                await PrintRooms(client);
                            }
                            else if (message.StartsWith("/whisper"))
                            {
                                await SendPrivateMessage(client, message);
                            }
                            else
                            {
                                await SendMessageToRoom(username, message, client.RoomName);
                            }
                            break;
                    }
                }
            }
        }

        private async Task HandleLogout(IClient client, string username)
        {
            await ServerPrivateMessage(client, "You have disconnected");
            await SendMessageToRoom(username, "has left the chat", client.RoomName);
            Console.WriteLine($"Server - {username} has disconnected");
            clients.Remove(client);
            await LeaveRoom(client);
            client.ClientSocket.Shutdown(SocketShutdown.Both);
            client.ClientSocket.Close();
        }

        private async Task SendClientList(IClient client)
        {
            string listOfOnlineClients = "The list of online clients are:";
            foreach (var currClient in clients)
            {
                listOfOnlineClients += $"\n<--> {currClient.Username}" + (currClient.Username == client.Username ? " (you)" : "");
            }
            await ServerPrivateMessage(client, listOfOnlineClients);
        }

        private async Task HandleCreateRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length == 2)
            {
                string roomName = parts[1];
                var room = new Room(roomName);
                rooms.Add(room);
                Console.WriteLine($"Room {roomName} was created by {client.Username}");
            }
        }

        private async Task HandleJoinRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            var room = rooms.FirstOrDefault(r => r.Name == parts[1]);
            if (room != null)
            {
                if (parts.Length == 2)
                {
                    rooms.FirstOrDefault(r => r.Name == client.RoomName).RemoveClientFromRoom(client);
                    Console.WriteLine($"Client {client.Username} has left room {client.RoomName}");
                    rooms.FirstOrDefault(r => r.Name == parts[1]).AddClientToRoom(client);
                    client.RoomName = parts[1];
                    Console.WriteLine($"Client {client.Username} has joined room {client.RoomName}");
                    await SendMessageToRoom("Server", $"{client.Username} has joined the room {parts[1]}", parts[1]);
                }
            }
            else
            {
                await ServerPrivateMessage(client, $"Room {parts[1]} doesn't exist");
            }
        }

        private async Task LeaveRoom(IClient client)
        {
            rooms.FirstOrDefault(r => r.Name == client.RoomName).RemoveClientFromRoom(client);
            await SendMessageToRoom("Server", "has left the room " + client.RoomName, client.RoomName);
            rooms.FirstOrDefault(r => r.Name == "Main").AddClientToRoom(client);
            client.RoomName = "Main";
            await SendMessageToRoom("Server", $"{client.Username} has joined the room {client.RoomName}", client.RoomName);
        }

        private async Task HandleInviteRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length >= 2)
            {
                string roomName = parts[1];
                var invitedClient = clients.FirstOrDefault(c => c.Username == client.Username && c.RoomName == roomName);
                if (invitedClient != null)
                {
                    await ServerPrivateMessage(client, $"You have been invited to room: {roomName}");
                }
            }
        }

        private async Task PrintRooms(IClient client)
        {
            string message = "";
            foreach (var room in rooms)
            {
                message += $"\nRoom {room.Name}:\n";
                foreach (var member in room.Members)
                {
                    message += $"\n| - <{member.Username}>" + (member.Username == client.Username ? " (you)" : "");
                }
                message += "\n";
            }
            await ServerPrivateMessage(client, message);
        }

        private async Task SendMessageToRoom(string username, string message, string roomName)
        {
            var room = rooms.FirstOrDefault(r => r.Name == roomName);
            if (room != null)
            {
                foreach (var member in room.Members)
                {
                    if (member.Username != username)
                    {
                        var response = $"{username}: {message}";
                        var responseByte = Encoding.UTF8.GetBytes(response);
                        await member.ClientSocket.SendAsync(responseByte, SocketFlags.None);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Room '{roomName}' does not exist.");
            }
        }

        private async Task ServerPrivateMessage(IClient client, string message)
        {
            var response = $"Server: {message}";
            var responseByte = Encoding.UTF8.GetBytes(response);
            await client.ClientSocket.SendAsync(responseByte, SocketFlags.None);
        }

        private async Task SendPrivateMessage(IClient client, string message)
        {
            var parts = message.Split(' ');
            var receiver = clients.FirstOrDefault(r => r.Username == parts[1]);
            var response = $"{client.Username} (whisper): {message}";
            var responseByte = Encoding.UTF8.GetBytes(response);
            await ServerPrivateMessage(receiver, parts[2]);
        }

    }
}
