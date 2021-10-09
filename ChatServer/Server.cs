using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Buffers;

namespace ChatServer
{
    
    class Server
    {
        private List<Socket> clientsList = new List<Socket>();
        
        public Server(string ip = "127.0.0.1", int port = 12345)
        {
            //Resolve connection data
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            activateAsyncListener(ipAddress, ipEndPoint);
        }

        private void activateListener(IPAddress ip, IPEndPoint endPoint)
        {
            // Data buffer for incoming data.  
            //This allocates memory and might cause memory problems later on
            //byte[] bytes = new byte[1024];

            using IMemoryOwner<byte> bytes = MemoryPool<byte>.Shared.Rent(1024);

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
                        int receivedBytes = handler.Receive(bytes.Memory.Span);
                        receivedMsg += Encoding.ASCII.GetString(bytes.Memory.Span);
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

        private void activateAsyncListener(IPAddress ip, IPEndPoint endPoint)
        {
            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(endPoint);
                listener.Listen();

                while (true)
                {
                    //accepting the connections
                    Console.WriteLine("Waiting for a connection");
                    Socket newClientConnection = listener.Accept();


                    //adding the connection to connections list and starting the message transfer
                    clientsList.Add(newClientConnection);
                    Task.Run(() => transferMessages(clientsList[clientsList.Count-1], clientsList.Count-1));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Caught Exception: {e}");
                Console.WriteLine("Terminating listener");
                return;
            }
        }


        //private void acceptConnections(Socket listener)
        //{
        //    while (true)
        //    {
        //        //accepting the connections
        //        Console.WriteLine("Waiting for a connection");
        //        Socket newClientConnection = listener.Accept();


        //        //adding the connection to connections list and starting the message transfer
        //        Task.Run(() => transferMessages(newClientConnection));
        //    }
        //}

        //private void addNewConnection(Socket newConnection)
        //{
        //    this.clientsList.Add(newConnection);
        //}

        private void transferMessages(Socket connection, int socketPosition)
        {
            Console.WriteLine($"Starting transfering messages to {connection}");
            while (true)
            {
                try
                {
                    //Receive message
                    //This might cause problems with memory later on
                    //byte[] receivedMessageEncoded = new byte[1024];

                    //So we are using this, this should create and release rented buffer which will dispose itself when not needed
                    using IMemoryOwner<byte> receivedMessageEncoded = MemoryPool<byte>.Shared.Rent(1024);

                    // Sometimes the memory span may contain leftover values, so we have to clear them
                    // Not sure how optimal this is, might be subject to change
                    // TODO: Read on memory span clearing
                    receivedMessageEncoded.Memory.Span.Clear();

                    connection.Receive(receivedMessageEncoded.Memory.Span);
                    Console.WriteLine($"Message in memory: {receivedMessageEncoded.Memory.Span.ToString()}");

                    //Decode it and print, this is for debug only
                    string receivedMessageDecoded = Encoding.ASCII.GetString(receivedMessageEncoded.Memory.Span);
                    Console.WriteLine($"Received message: {receivedMessageDecoded}");

                    //Send it to all clients

                    for (int i = 0; i < this.clientsList.Count; i++)
                    {
                        //if (clientsList[i] == connection) { continue; }
                        clientsList[i].Send(receivedMessageEncoded.Memory.Slice(0, receivedMessageEncoded.Memory.Span.Length).Span);
                        Console.WriteLine("Sent message to client");
                    }
                }
                //Client ended connection (either knowingly or by some connection issue)
                catch ( SocketException se)
                {
                    Console.WriteLine($"Encountered SocketException: {se}");
                    Console.WriteLine("Socket timed out, severing the connection");
                    connection.Close();
                    clientsList.RemoveAt(socketPosition);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Encountered unintended exception: {e}");
                    Console.WriteLine("Severing the connection");
                    connection.Dispose();
                    clientsList.RemoveAt(socketPosition);
                    return;
                }
                


            }
        }

    }
}
