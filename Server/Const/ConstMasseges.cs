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
                                      "/private <username> - enter a private chat with user specified\n" +
                                      "/help - Display this list of commands";
    
    
    public const string DisconnectedMassege = "You Have Disconnected";
    
    public const string ListOfOnlineClientsAre = "The List Of Online Clients Are:";
    public const string ListOfOnlineClientsChanges = "The list of online clients are has changed\n";
    
    public const string ServerConst = "Server";

    public const string ErrorEmptyRoomMassage = "Invalid. The Command Should Be Used As Such: /jroom [roomName]";
    public const string TheRoomDosentExist = "Invalid. The Room Specified Dosent Exist";
    
    
    public const string ErrorEmptyInviteRoomMassage = "Invalid. The Command Should Be Used As Such: /iroom [roomName]";

    public const string ErrorCannotEnterPrivateRoom = "Error, Cannot Enter Private Room, Type A Vailid Room";
    public const string InvitationToRoomMassage = "You have been invited to room: ";

    public const string CannotCreateEmptyRoom = "Error Cannot Create Empty Room, Specify A Name";
    public const string CannotCreateRoomAlreadyExist = "Error Cannot Create A Room, That Already Exist";
    public const string RoomWasCreated = "Room Was Successfully Created";
    public const string UnknownCommand = "An Unknown Was Typed";






}