using Server.Models;

namespace Server.Interfaces
{
    public interface IChatServer
    {
        
        public List<Client> clients { get; set; }
        public List<Room> rooms { get; set; }
        Task StartAsync();
        
        Task SendPrivateMessage(Client client, string message);
        Task ServerPrivateMessage(Client client, string message);
        Task PrintToAll(Client client, string massege);
        Task LoadMessages(Client client, string message);
    }
}