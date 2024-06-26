
using Server.Models;
namespace Server.Interfaces
{
    public interface IRoom
    {
        void AddClientToRoom(Client client);
        void RemoveClientFromRoom(Client client);
    }
}