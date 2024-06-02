namespace Server.Interfaces
{
    public interface IRoom
    {
        string Name { get; }
        List<IClient> Members { get; }
        void AddClientToRoom(IClient client);
        void RemoveClientFromRoom(IClient client);
    }
}