using System.Net;
using System.Net.Sockets;
using System.Text;
using Server.Const;
using Server.Interfaces;
using Server.Interfaces.ClientHandler;
using Server.Interfaces.RoomsAndChats;
using Server.Models;
using Server.Services.RoomAndChats;
using Server.Services.RoomAndChats.Rooms;

namespace Server.Services
{
    public class ChatServer : IChatServer
    {
        public ChatServer(ICleintHandler cleintHandler, IRoomServices roomServices, IPrivateChatHandler privateChatHandler)
        {
            _cleintHandler = cleintHandler;
            _roomServices = roomServices;
            _privateChatHandler = privateChatHandler;
            _messageFormatter = new MessageFormatter();
            clients = new List<IClient>();
            rooms = new List<IRoom>();
        }

        private ICleintHandler _cleintHandler;
        private IRoomServices _roomServices;
        private IPrivateChatHandler _privateChatHandler;
        private IMessageFormatter _messageFormatter;


        public ChatServer()
        {
            clients = new List<IClient>();
            rooms = new List<IRoom>();
            _roomServices = new RoomServices(this);
            _messageFormatter = new MessageFormatter();
            _privateChatHandler = new PrivateChatHandler(this,_roomServices);
            _cleintHandler = new ClientHandler.ClientHandler(this,_roomServices,_privateChatHandler);
        }

        public List<IClient> clients { get; set; }
        public List<IRoom> rooms { get; set; }

        public async Task StartAsync()
        {
            IPHostEntry ipEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());
            IPAddress ip = ipEntry.AddressList[1];
            Console.WriteLine($"IP is {ip}");
            IPEndPoint ipEndPoint = new(ip, 1234);

            using Socket server = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ipEndPoint);
            server.Listen();
            Console.WriteLine(ConstMasseges.PortListening);

            IRoom defaultRoom = new Room(ConstMasseges.DefaultRoom);
            rooms.Add(defaultRoom);


            while (true)
            {
                Socket handler = await server.AcceptAsync();
                var buffer = new byte[1024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var username = Encoding.UTF8.GetString(buffer, 0, received).Trim();

                IClient client = new Models.Client(handler, username, defaultRoom.Name);
                defaultRoom.AddClientToRoom(client);
                clients.Add(client);
                await _cleintHandler.UpdatedClientList(client);

                Console.WriteLine($"The client {client.Username} has connected to the server");
                _roomServices.ExistingRooms();
                _ = Task.Run(() => _cleintHandler.HandleClient(client));
            }

        }

        public async Task ServerPrivateMessage(IClient client, string message)
        {

            var response = _messageFormatter.Response(ConstMasseges.ServerConst, message);
            var responseByte = Encoding.UTF8.GetBytes(response);
            await client.ClientSocket.SendAsync(responseByte, SocketFlags.None);
        }
        
        public async Task SendPrivateMessage(IClient client, string message)
        {

            var response = _messageFormatter.Response("", message);
            var responseByte = Encoding.UTF8.GetBytes(response);    
            await client.ClientSocket.SendAsync(responseByte, SocketFlags.None);
        }
        public async Task LoadMessages(IClient client, string message)
        {


            var responseByte = Encoding.UTF8.GetBytes(message);    
            await client.ClientSocket.SendAsync(responseByte, SocketFlags.None);
        }



        public async Task PrintToAll(IClient client,string massege)
        {
            foreach (var member in clients)
            {
                if (client.Username == member.Username)
                {
                    var response = _messageFormatter.Response(ConstMasseges.ServerConst, massege);
                    var responseByte = Encoding.UTF8.GetBytes(response);
                    await member.ClientSocket.SendAsync(responseByte, SocketFlags.None);
                }
                else
                {
                    var response = _messageFormatter.Response(ConstMasseges.ServerConst, massege);
                    var responseByte = Encoding.UTF8.GetBytes(response);
                    await member.ClientSocket.SendAsync(responseByte, SocketFlags.None);
                }
                
            }
        }
        
    }
}
