namespace Server.Interfaces
{
    public interface IChatServer
    {

        List<IClient> clients { get; set; }
        List<IRoom> rooms { get; set; }
        Task StartAsync();
        Task ServerPrivateMessage(IClient client, string message);
        Task PrintToAll(IClient client, string massege);
    }
}