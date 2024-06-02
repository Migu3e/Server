using Server.Interfaces;

namespace Server.Models
{
    public class Room : IRoom
    {
        public string Name { get; private set; }
        public List<IClient> Members { get; private set; }

        public Room(string name)
        {
            Name = name;
            Members = new List<IClient>();
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