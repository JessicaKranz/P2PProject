using Datenmodelle;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CommonLogic
{
    public class TcpConnection
    {
        static List<TcpClient> tcpClients = new List<TcpClient>();

        static bool go_on = true;

        public void StartServersAndClients(List<IP> serverAddresses, List<IP> tcpClientAdresses)
        {
            //Start all servers in separate threads
            serverAddresses.ForEach(address => new Thread(o => TcpConnection.Server(address.port)).Start());

            //Servers must be running before clients may connect
            Thread.Sleep(1000);

            //Create all tcpClients
            tcpClientAdresses.ForEach(address => tcpClients.Add(new TcpClient(address.address, address.port)));
            //Start one threads that manages the connection to all communication partners
            new Thread(o => TcpConnection.Client(tcpClients)).Start();
        }


        public static void Join(TcpClient knownTcpClient)
        {          
            string message = "Join";         
            //stream readers can only process streams of known length
            message = message.PadRight(128 + 4 - message.Length, '-');

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            try
            {
                NetworkStream stream = knownTcpClient.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                // Receive the TcpServer.response.

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                //stream.Close();
                //knownTcpClient.Close();
                Console.WriteLine("Disconnected");
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public static void Client(List<TcpClient> clients)
        {
            while (go_on)
            {
                Thread.Sleep(300);
                string line = Console.ReadLine();
                if (line.Length != 0)
                {                 
                    //stream readers can only process streams of known length
                    line = line.PadRight(128 + 4 - line.Length, ' ');

                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(line);

                    foreach (var client in clients)
                    {
                        try
                        {
                            NetworkStream stream = client.GetStream();

                            // Send the message to the connected TcpServer.
                            stream.Write(data, 0, data.Length);

                            // Receive the TcpServer.response.

                            // String to store the response ASCII representation.
                            String responseData = String.Empty;

                            // Read the first batch of the TcpServer response bytes.
                            Int32 bytes = stream.Read(data, 0, data.Length);
                            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                            Console.WriteLine("Received: {0}", responseData);

                            //if (line.Equals("quit"))
                            //{
                            //    go_on = false;
                            //    // Close everything.
                            //stream.Close();
                            //client.Close();
                            //Console.WriteLine("Disconnected");
                            //}
                            //}

                        }
                        catch (ArgumentNullException e)
                        {
                            Console.WriteLine("ArgumentNullException: {0}", e);
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine("SocketException: {0}", e);
                        }
                    }
                }
            }
        }



        public static void Server(Int32 port)
        {
            Console.WriteLine("Hello from the server");

            // todo do better
            string joinMessage = "Join";
            joinMessage = joinMessage.PadRight(128 + 4 - joinMessage.Length, '-');

            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        
                        if (data == joinMessage)
                        {
                            OnPeerJoin(data, stream);
                        }
                        else
                        {
                            OnChatMessageReceived(data, stream);
                        }
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }


        public static void OnPeerJoin(string data, NetworkStream stream)
        {
            // select one known neighbor or the own address and send it back
            // ...

            byte[] msg = System.Text.Encoding.ASCII.GetBytes("HeyWelcome");
            // Send back a response.
            stream.Write(msg, 0, msg.Length);
            data = System.Text.Encoding.ASCII.GetString(msg, 0, msg.Length);

            Console.WriteLine("Sent: {0}", data);
            //return an IPEndPoint as next Peer to contact
        }    
        
        public static void OnChatMessageReceived(string data, NetworkStream stream)
        {
            // Process the data sent by the client.
            data = data.ToUpper();

            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
            // Send back a response.
            stream.Write(msg, 0, msg.Length);
            Console.WriteLine("Sent: {0}", data);
        }
    }
}
