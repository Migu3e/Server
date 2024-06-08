using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Server.Const;
using Server.Interfaces;
using Server.Models;

namespace Server.Services;

public class RoomServices : IRoomServices
{
    private readonly IChatServer _chatServer;

    public RoomServices(IChatServer services)
    {
        _chatServer = services;
    }

    public async Task SendMessageToRoom(string username, string message, string roomName)
    {
        var room = _chatServer.rooms.FirstOrDefault(r => r.Name == roomName);
        if (room != null)
        {
            if (roomName == "Main")
            {
                _chatServer.ServerPrivateMessage(_chatServer.clients.FirstOrDefault(p => username == p.Username),"Cannot Massage In Room Main");
            }
            else
            {
                foreach (var member in room.Members)
                {
                    if (member.Username != username)
                    {
                        var response = $"<{DateTime.Now} - {username}> {message}";

                        var responseByte = Encoding.UTF8.GetBytes(response);
                        await member.ClientSocket.SendAsync(responseByte, SocketFlags.None);
                    }
                    var newresponse = $"<{DateTime.Now} - {username}> {message}\n";
                    Console.WriteLine(newresponse);
                    room.Messages.Add(newresponse);
                }
            }
                
        }
        else
        {
            Console.WriteLine($"Room '{roomName}' does not exist.");
        }
    }


    public async Task HandleCreateRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length == 2)
            {
                string roomName = parts[1];
                var room = new Room(roomName);
                _chatServer.rooms.Add(room);
                Console.WriteLine($"Room {roomName} was created by {client.Username}");
                await _chatServer.PrintToAll(client,$"Room {roomName} was created by {client.Username}");
            }
        }
    public async Task HandleJoinRoom(IClient client, string message)
    {
        var parts = message.Split(' ');
        if (parts.Length < 2)
        {
            await _chatServer.ServerPrivateMessage(client, ConstMasseges.ErrorEmptyRoomMassage);
            return;
        }

        var room = _chatServer.rooms.FirstOrDefault(r => r.Name == parts[1]);
        if (room == null)
        {
            await _chatServer.ServerPrivateMessage(client, $"Room {parts[1]} doesn't exist");
            return;
        }

        if (room.Name.Contains("|private|"))
        {
            await _chatServer.ServerPrivateMessage(client, ConstMasseges.ErrorCannotEnterPrivateRoom);
            return;
        }

        _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName).RemoveClientFromRoom(client);
        Console.WriteLine($"Client {client.Username} has left room {client.RoomName}");
        _chatServer.rooms.FirstOrDefault(r => r.Name == parts[1]).AddClientToRoom(client);
        client.RoomName = parts[1];
        Console.WriteLine($"Client {client.Username} has joined room {client.RoomName}");
        foreach (var existingMessage in room.Messages)
        {
            await _chatServer.PrivateMessage(client, existingMessage);
        }
        await SendMessageToRoom(ConstMasseges.ServerConst, $"{client.Username} has joined the room {parts[1]}", parts[1]);
    }


    public async Task LeaveRoom(IClient client)
        {
            _chatServer.rooms.FirstOrDefault(r => r.Name == client.RoomName).RemoveClientFromRoom(client);
            await SendMessageToRoom(ConstMasseges.ServerConst, client.Username+ " has left the room " + client.RoomName, client.RoomName);
            _chatServer.rooms.FirstOrDefault(r => r.Name == "Main").AddClientToRoom(client);
            client.RoomName = "Main";
            await SendMessageToRoom(ConstMasseges.ServerConst, $"{client.Username} has joined the room {client.RoomName}", client.RoomName);
        }

    public async Task HandleInviteRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length >= 2)
            {
                string roomName = parts[1];
                var invitedClient = _chatServer.clients.FirstOrDefault(c => c.Username == client.Username && c.RoomName == roomName);
                if (invitedClient != null)
                {
                    await _chatServer.ServerPrivateMessage(client, ConstMasseges.InvitationToRoomMassage + roomName);
                }
            }
        }

    public async Task PrintRooms(IClient client)
        {
            string message = "";
            foreach (var room in _chatServer.rooms)
            {
                if (room.Name.StartsWith("|private|"))
                {
                    
                }
                else
                {
                    message += $"\nRoom {room.Name}";
                    foreach (var member in room.Members)
                    {
                        message += $"\n| <{member.Username}>" + (member.Username == client.Username ? " (you)" : "");
                    }
                    message += "\n";
                }
            }
            await _chatServer.ServerPrivateMessage(client, message);
        }
        
        
        

}