using Server.Interfaces;

namespace Server.Models
{
    public class Room : IRoom
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public List<IClient> Members { get; private set; }

        public List<string> Messages { get; set; }

        public Room(string name)
        {
            Password = "";
            Name = name;
            Members = new List<IClient>();
            Messages = new List<string>();
        }
        public Room(string name, string password)
        {
            Password = password;
            Name = name;
            Members = new List<IClient>();
            Messages = new List<string>();
        }

        public void AddClientToRoom(IClient client)
        {
            Members.Add(client);
        }

        public void RemoveClientFromRoom(IClient client)
        {
            Members.Remove(client);
        }
    }
}