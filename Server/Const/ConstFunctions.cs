using System.Net.Sockets;
using System.Text;
using Server.Interfaces;

namespace Server.Const;

public static class ConstFunctions
{
    public static string Response(string username,string message)
    {
        return $"<{DateTime.Now} - {username}> {message}\n";
    }
    public static string RoomWasDeleted(string roomName)
    {
        return $"Room '{roomName}' has been deleted\n";
    }
    public static string RoomWasCreated(string username,string roomName)
    {
        return $" {username} has created room '{roomName}'\n";
    }
    public static string ClientJoinedRoom(string roomName,string clientName)
    {
        return $"{clientName} has joined the room {roomName}\n";
    }
    public static string ClientLeftRoom(string roomName,string clientName)
    {
        return $"{clientName} has left the room {roomName}\n";
    }

    public static string InviteToRoom(string roomName, string clientName)
    {
        return $"You have invited {clientName} to join room {roomName}.";
    }
    public static string InviteToRoomMessege(string roomName, string clientName)
    {
        return $"{clientName} has invited you to join room {roomName}.";
    }
    public static string AllClientListMessage(List<string> onlineClients, List<string> offlineClients, string currentUser)
    {
        string clientList = ConstMasseges.ListOfOnline;

        foreach (var client in onlineClients)
        {
            clientList += $"\n<--> {client}" + (client == currentUser ? " (you)" : "");
        }

        clientList += ConstMasseges.ListOfOffline;

        foreach (var client in offlineClients)
        {
            clientList += $"\n<--> {client} (offline)";
        }

        return clientList;
    }
    public static string UpdatedClientListMessage(List<string> onlineClients, string newClient)
    {
        string clientList = ConstMasseges.ListOfOnlineClientsChanges;

        foreach (var client in onlineClients)
        {
            clientList += $"<--> {client}" + (client == newClient ? " (Just Joined)\n" : "\n");
        }

        return clientList;
    }
    public static string ClientListMessage(List<string> onlineClients, string currentUser)
    {
        string clientList = ConstMasseges.ListOfOnlineClientsAre;

        foreach (var client in onlineClients)
        {
            clientList += $"\n<--> {client}" + (client == currentUser ? " (you)" : "");
        }

        return clientList;
    }


    public static async Task<string[]> GetNameAndMessageParts(Socket handler)
    {
        var buffer = new byte[1024];
        int bytesReceived = await handler.ReceiveAsync(buffer, SocketFlags.None);

        // Check if the connection was closed
        if (bytesReceived == 0)
        {
            return null;
        }

        var receivedString = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

        // Split the received string into parts based on '|'
        var parts = receivedString.Split('|');

        return parts;
    }
    public static string GenerateRoomListMessage(List<IRoom> rooms, string currentUsername)
    {
        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(ConstMasseges.RoomListHeader);

        foreach (var room in rooms.Where(r => !ConstCheckCommands.IsPrivateRoom(r.Name)))
        {
            messageBuilder.AppendLine($"Room {room.Name}");
            foreach (var member in room.Members)
            {
                string memberLine = $"| <{member.Username}>";
                if (member.Username == currentUsername)
                {
                    memberLine += ConstMasseges.YouIndicator;
                }
                messageBuilder.AppendLine(memberLine);
            }
            messageBuilder.AppendLine();
        }

        return messageBuilder.ToString();
    }


    public static string UserHasEnteredPrivateChat(string username)
    {
        return $"{username} has entered the private chat with you.";
    }

    public static string UserJoinedPrivateRoom(string username, string roomName)
    {
        return $"{username} has joined the private room {roomName}.";
    }

    public static string RoomDoesNotExist(string targetUsername)
    {
        return $"Room with {targetUsername} doesn't exist.";
    }
    




}