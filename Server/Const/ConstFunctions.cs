namespace Server.Const;

public class ConstFunctions
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
}