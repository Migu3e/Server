using System.Net.Sockets;
using Server.Interfaces;

namespace Server.Models
{
    public class Client : IClient
    {
        public Socket ClientSocket { get; private set; }
        public string Username { get; private set; }
        public string RoomName { get; set; }

        public Client(Socket socket, string username, string roomName)
        {
            ClientSocket = socket;
            Username = username;
            RoomName = roomName;
        }
    }
}