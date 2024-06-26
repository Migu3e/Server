using Server.Interfaces;
using Server.Models;
namespace Server.Models
{
    public class Room : IRoom
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public List<Client> Members { get; private set; }

        public List<string> Messages { get; set; }

        public Room(string name)
        {
            Password = "";
            Name = name;
            Members = new List<Client>();
            Messages = new List<string>();
        }
        public Room(string name, string password)
        {
            Password = password;
            Name = name;
            Members = new List<Client>();
            Messages = new List<string>();
        }

        public void AddClientToRoom(Client client)
        {
            Members.Add(client);
        }

        public void RemoveClientFromRoom(Client client)
        {
            Members.Remove(client);
        }
    }
}