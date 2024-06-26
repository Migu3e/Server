using System.Net.Sockets;

namespace Server.Interfaces.ClientHandler;

public interface IClientHandlerHelper
{
    Task<string[]> GetNameAndMessageParts(Socket handler);
}