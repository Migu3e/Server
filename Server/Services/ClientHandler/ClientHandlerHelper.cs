using System.Net.Sockets;
using System.Text;
using Server.Interfaces.ClientHandler;

namespace Server.Services.ClientHandler;

public class ClientHandlerHelper : IClientHandlerHelper
{
    public async Task<string[]> GetNameAndMessageParts(Socket handler)
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
}