namespace Server.Interfaces
{
    public interface IChatServer
    {

        List<IClient> clients { get; set; }
        List<IRoom> rooms { get; set; }
        Task StartAsync();
        
        Task PrivateMessage(IClient client, string message);
        Task ServerPrivateMessage(IClient client, string message);
        Task PrintToAll(IClient client, string massege);
        Task LoadMessages(IClient client, string message);
    }
}