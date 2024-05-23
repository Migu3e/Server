using System.Net.Sockets;

class Client
{
    public Socket client;
    public string username;
    public string roomName; // New property to track room

    public Client(Socket socket, string name)
    {
        client = socket;
        username = name;
        roomName = ""; // Initially not in any room
    }
}