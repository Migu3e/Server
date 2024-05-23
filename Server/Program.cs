
using System.Net;
using System.Net.Sockets;
using System.Text;

IPHostEntry ipEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());

// extracting local host ip (127.0.1.1)
IPAddress ip = ipEntry.AddressList[0];

// connect the server socket to client socket
IPEndPoint ipEndPoint = new(ip,1234);

using Socket server = new
(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp
);

server.Bind(ipEndPoint);
server.Listen();
Console.WriteLine("server has started lisining on port 1234");

var handler = await server.AcceptAsync();

while (true)
{
    var buffer = new byte[1_042];
    
    // received messege as a byte
    var recived = await handler.ReceiveAsync(buffer, SocketFlags.None);

    var mesageString = Encoding.UTF8.GetString(buffer, 0, recived);

    if (mesageString != null)
    {
        Console.WriteLine("Message from client: " + mesageString);
        var response = "Message Recived!";
        var responseByte = Encoding.UTF8.GetBytes(response);

        await handler.SendAsync(responseByte, SocketFlags.None);
    }
}