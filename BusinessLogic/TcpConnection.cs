using Datenmodelle;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CommonLogic
{
    public class TcpConnection
    {
        List<TcpClient> tcpClients = new List<TcpClient>();

        bool go_on = true;
        Random random = new Random();

        public void StartServersAndClients(PeerData peer)
        {
            //Start all servers in separate threads
            peer.serverAddresses.ForEach(address => new Thread(o => this.Server(peer, address.port)).Start());
            peer.serverAddresses.ForEach(address => Console.WriteLine("Started serving on {0}:{1}", "127.0.0.1", address.port));

            //Servers must be running before clients may connect
            Thread.Sleep(1000);

            //Create all tcpClients
            peer.tcpClientAddresses.ForEach(address => tcpClients.Add(new TcpClient(address.address, address.port)));
            //Start one threads that manages the connection to all communication partners
            new Thread(o => this.Client(tcpClients)).Start();
        }


        public void Join(IP OriginIp, IP IpToJoin)
        {
            MessageData messageData = new MessageData
            {
                Type = MessageData.Types.JoinRequest,
                Destination = IpToJoin,
                Source = OriginIp,
                Ttl = 5
            };

            //stream readers can only process streams of known length
            string message = messageData.ToJson();
            var num = 128;
            message = message.PadRight(num, '-');

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            try
            {
                TcpClient tcpClient = new TcpClient(messageData.Destination.address, messageData.Destination.port);
                NetworkStream stream = tcpClient.GetStream();

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
                //Console.WriteLine("Disconnected");
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

        public void Client(List<TcpClient> clients)
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



        public void Server(PeerData self, Int32 port)
        {
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
                        try
                        {
                            MessageData message = JsonConvert.DeserializeObject<MessageData>(data.TrimEnd('-'));
                            switch (message.Type)
                            {
                                case MessageData.Types.JoinRequest:
                                    OnPeerJoinRequest(self, message, stream);
                                    break;
                                case MessageData.Types.JoinResponse:
                                    OnPeerJoinResponse();
                                    break;
                                default:
                                    OnChatMessageReceived(data, stream);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Could not deserialize malformed message. Message was {0}. Failed with {1}", data, ex.Message);
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

        private void OnPeerJoinResponse()
        {
            throw new NotImplementedException();
        }

        public void OnPeerJoinRequest(PeerData self, MessageData message, NetworkStream stream)
        {
            //if peer has no neighbors, it'll transmits its own address
            if (self.tcpClientAddresses.Count == 0)
            {
                string send = self.serverAddresses[0].ToJson();
                //stream readers can only process streams of known length
                send = send.PadRight(128, '-');

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(send);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                string data = System.Text.Encoding.ASCII.GetString(msg, 0, msg.Length);

                Console.WriteLine("Sent: {0}", data);
            }
            else
            {
                // select one known neighbor and send it back
                // ...
                if (message.Ttl > 0)
                {
                    message.Ttl--;
                    // todo does the clients neighbor have an open port to speak with us?
                    var nextRandomWalkStep = self.tcpClientAddresses.ElementAt(random.Next(self.tcpClientAddresses.Count));
                    Join(message.Source, nextRandomWalkStep);
                }
                else
                {

                }
            }            
        }    
        
        public void OnChatMessageReceived(string data, NetworkStream stream)
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
