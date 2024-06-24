using Server.Interfaces;
using MongoDB.Driver;
using Server.Const;
using Server.MongoDB;
using Server.Helpers;

namespace Server.Services
{
    public class PrivateChatHandler : IPrivateChatHandler
    {
        private readonly IChatServer _chatServer;
        private readonly IRoomServices _roomServices;
        private readonly IMessageFormatter _messageFormatter;
        private readonly IPrivateChatHelper _privateChatHelper;

        public PrivateChatHandler(IChatServer server, IRoomServices roomServices)
        {
            _chatServer = server;
            _roomServices = roomServices;
            _messageFormatter = new MessageFormatter();
            _privateChatHelper = new PrivateChatHelper(_chatServer, _roomServices, _messageFormatter);
        }
        
        public async Task HandleJoinPrivateRoom(IClient client, string message)
        {
            var parts = message.Split(' ');
            if (parts.Length < 2)
            {
                await _chatServer.ServerPrivateMessage(client, ConstMasseges.InvalidFormat);
                return;
            }

            string targetUsername = parts[1];
            if (client.Username == targetUsername)
            {
                await _chatServer.ServerPrivateMessage(client, ConstMasseges.JoinPrivateChatWithSelf);
                return;
            }

            var room = _chatServer.rooms.FirstOrDefault(r => r.Name.Contains($".{targetUsername}.") && r.Name.Contains($".{client.Username}."));
            if (room != null)
            {
                await _privateChatHelper.LeaveCurrentRoom(client);
                await _privateChatHelper.NotifyTargetUser(targetUsername, client.Username);
                await _privateChatHelper.JoinRoom(client, room, targetUsername);
            }
            else
            {
                await _chatServer.ServerPrivateMessage(client, _messageFormatter.RoomDoesNotExist(targetUsername));
            }
        }
    }
}