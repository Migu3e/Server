namespace Server.Interfaces
{
    public interface IRoom
    {
        string Name { get; }
        List<IClient> Members { get; }
        List<string> Messages { get; }
        void AddClientToRoom(IClient client);
        void RemoveClientFromRoom(IClient client);
    }
}