using System.Net.Sockets;

namespace Server.Interfaces
{
    public interface IClient
    {
        Socket ClientSocket { get; }
        string Username { get; }
        string RoomName { get; set; }
    }
}