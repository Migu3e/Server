using System.Net.Sockets;

namespace Server.Interfaces;

public interface IClientHandlerHelper
{
    Task<string[]> GetNameAndMessageParts(Socket handler);
}