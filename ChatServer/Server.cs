using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ChatServer
{
    
    class Server
    {
        private List<Socket> clientsList = new List<Socket>();
        
        public Server()
        {
            //Resolve connection data
            IPHostEntry ipHost = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 12345);
            activateListener(ipAddress, ipEndPoint);
        }

        private void activateListener(IPAddress ip, IPEndPoint endPoint)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            //Create server listener
            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(endPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = listener.Accept();
                    string receivedMsg = null;

                    while (true)
                    {
                        int receivedBytes = handler.Receive(bytes);
                        receivedMsg += Encoding.ASCII.GetString(bytes, 0, receivedBytes);
                        if (receivedMsg.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    Console.WriteLine($"Message received: {receivedMsg}");

                    byte[] msg = Encoding.ASCII.GetBytes("Response from server.<EOF>");

                    handler.Send(msg);
                    handler.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}
