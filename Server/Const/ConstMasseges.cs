namespace Server.Const;

public class ConstMasseges
{
    public const string PortListening = "Server Has Started Listening To Port 1234";
    public const string HelpMessage = "The commands are:\n" +
                                      "/list - Display the connected users\n" +
                                      "/logout - Disconnect from the server\n" +
                                      "/croom <roomname> - Create a new room with the specified name\n" +
                                      "/jroom <roomname> - Join an existing room with the specified name\n" +
                                      "/iroom <username> <roomname> - Invite a user to a specified room\n" +
                                      "/leave - Leave the current room and return to the main room\n" +
                                      "/list rooms - List all rooms and their members\n" +
                                      "/whisper <username> <message> - Send a private message to a specific user\n" +
                                      "/help - Display this list of commands";
    
    
    public const string DisconnectedMassege = "You Have Disconnected";
    
    public const string ListOfOnlineClientsAre = "The List Of Online Clients Are:";
    public const string ListOfOnlineClientsChanges = "The list of online clients are has changed\n";

    


}