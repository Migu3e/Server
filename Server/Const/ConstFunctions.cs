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
}