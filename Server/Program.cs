
using System.Net;
using System.Net.Sockets;
using System.Text;

IPHostEntry ipEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());

// extracting local host ip (127.0.1.1)
IPAddress ip = ipEntry.AddressList[0];

// connect the server socket to client socket
IPEndPoint ipEndPoint = new(ip,1234);
