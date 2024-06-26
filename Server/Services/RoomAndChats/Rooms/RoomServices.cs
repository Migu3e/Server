using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Server.Const;
using Server.Encryption;
using Server.Interfaces;
using Server.Interfaces.RoomsAndChats;
using Server.Models;
using Server.MongoDB;

namespace Server.Services.RoomAndChats.Rooms
{
    public class RoomServices : IRoomServices
    {
        private readonly IChatServer _chatServer;
        private readonly IMessageFormatter _messageFormatter;
        private readonly IRoomRepository _roomRepository;
        
        
        public RoomServices(IChatServer chatServer)
        {
            _chatServer = chatServer;
            _messageFormatter = new MessageFormatter();
            _roomRepository = new RoomRepository();
        }

        public async Task ExistingRooms()
        {
            var rooms = await _roomRepository.GetAllRooms();
            foreach (var room in rooms)
            {
                if (_chatServer.rooms.Any(p => p.Name == room.RoomName)) continue;
                _chatServer.rooms.Add(new Room(room.RoomName));
            }
        }

        public async Task SendMessageToRoom(string username, string message, string roomName)
        {
            Client client = _chatServer.clients.FirstOrDefault(p => username == p.Username);
            var room = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
            if (room == null)
            {
                Console.WriteLine($"Room '{roomName}' does not exist.");
                return;
            }


            if (roomName == ConstMasseges.DefaultRoom)
            {
                await _chatServer.ServerPrivateMessage(client, ConstMasseges.CannotMessageInMain);
                return;
            }

            var response = _messageFormatter.Response(username, message);
            foreach (var member in room.Members)
            {
                if (member.Username != username)
                {
                    var responseByte = Encoding.UTF8.GetBytes(response);
                    await member.ClientSocket.SendAsync(responseByte, SocketFlags.None);
                }
            }

            room.Messages.Add(response);
            await _roomRepository.UpdateRoomMessages(roomName, response);
        }

        public async Task HandleCreateRoom(Client client, string message)
        {
            var parts = message.Split(' ');
            var validationMessage = ConstCheckCommands.CanCreateRoom(message, _chatServer.rooms);

            if (validationMessage != ConstMasseges.RoomWasCreated)
            {
                await _chatServer.SendPrivateMessage(client, validationMessage);
                return;
            }

            var roomName = parts[1];
            var password = parts[2];
            var room = new Room(roomName, password);
            _chatServer.rooms.Add(room);

            Console.WriteLine($"Room {roomName} was created by {client.Username}");
            await _chatServer.PrintToAll(client, _messageFormatter.RoomWasCreated(client.Username, roomName));

            var encrypt = new Encrypt();
            var roomDb = new RoomDB { RoomName = roomName, Password = encrypt.encrypt(password), MList = new List<string>() };
            await _roomRepository.InsertRoom(roomDb);
        }

        public async Task HandleJoinRoom(Client client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length < 3)
            {
                await _chatServer.SendPrivateMessage(client, ConstMasseges.IncorrectNamePassword);
                return;
            }

            var roomName = parts[1];
            var password = parts[2];
            var roomFromDb = await _roomRepository.GetRoomByName(roomName);

            if (roomFromDb == null || new Decrypt().decrypt(roomFromDb.Password) != password)
            {
                await _chatServer.SendPrivateMessage(client, ConstMasseges.IncorrectNamePassword);
                return;
            }

            var room = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
            var validationMessage = ConstCheckCommands.CanJoinRoom(message, room);

            if (validationMessage != "true")
            {
                await _chatServer.SendPrivateMessage(client, validationMessage);
                return;
            }

            var currentRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName);
            currentRoom?.RemoveClientFromRoom(client);

            var newRoom = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
            newRoom?.AddClientToRoom(client);
            client.RoomName = roomName;

            foreach (var msg in roomFromDb.MList)
            {
                await _chatServer.LoadMessages(client, msg);
            }

            await SendMessageToRoom(ConstMasseges.ServerConst, _messageFormatter.ClientJoinedRoom(roomName, client.Username), roomName);
        }

        public async Task HandleDeleteRoom(string message, Client client)
        {
            var parts = message.Split(' ');
            var roomName = parts[1];

            var validationMessage = ConstCheckCommands.CanDeleteRoom(message, _chatServer.rooms);
            if (validationMessage != ConstMasseges.DeletedRoom)
            {
                await _chatServer.ServerPrivateMessage(client, validationMessage);
                return;
            }

            var room = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
            if (room != null)
            {
                _chatServer.rooms.Remove(room);
                await _roomRepository.DeleteRoom(roomName);
                await _chatServer.PrintToAll(client, _messageFormatter.RoomWasDeleted(roomName));
                Console.WriteLine($"Room '{roomName}' has been deleted.");
            }
        }

        public async Task LeaveRoom(Client client)
        {
            await SendMessageToRoom(ConstMasseges.ServerConst, _messageFormatter.ClientLeftRoom(client.RoomName, client.Username), client.RoomName);
            _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName)?.RemoveClientFromRoom(client);
            _chatServer.rooms.FirstOrDefault(r => r.Name == ConstMasseges.DefaultRoom)?.AddClientToRoom(client);
            client.RoomName = ConstMasseges.DefaultRoom;
            await SendMessageToRoom(ConstMasseges.ServerConst, _messageFormatter.ClientJoinedRoom(client.RoomName, client.Username), client.RoomName);
        }

        public async Task HandleInviteRoom(Client client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length < 3)
            {
                await _chatServer.SendPrivateMessage(client, ConstMasseges.InvalidFormat);
                return;
            }

            var roomName = parts[1];
            var invitedUsername = parts[2];

            if (ConstCheckCommands.CanInviteToRoom(message, client) != "true")
            {
                await _chatServer.SendPrivateMessage(client, ConstCheckCommands.CanInviteToRoom(message, client));
                return;
            }

            var invitedClient = _chatServer.clients.FirstOrDefault(c => c.Username == invitedUsername);
            if (invitedClient != null)
            {
                await _chatServer.ServerPrivateMessage(invitedClient, _messageFormatter.InviteToRoomMessege(roomName, client.Username));
                await _chatServer.SendPrivateMessage(client, _messageFormatter.InviteToRoom(roomName, invitedUsername));
            }
            else
            {
                await _chatServer.SendPrivateMessage(client, ConstMasseges.ClientIsOffline);
            }
        }

        public async Task PrintRooms(Client client)
        {
            var message = _messageFormatter.GenerateRoomListMessage(_chatServer.rooms, client.Username);
            await _chatServer.ServerPrivateMessage(client, message);
        }
    }
}
