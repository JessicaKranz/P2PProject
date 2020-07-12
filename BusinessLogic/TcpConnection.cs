using Datenmodelle;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Reflection;
using System.Text;

namespace BusinessLogic
{
    public class TcpConnection
    {
        const int MESSAGE_MAX_LENGTH = 1024;
        readonly IPAddress LocalAddr = IPAddress.Parse("127.0.0.1");

        bool go_on = true;
        readonly Random random = new Random();

        public void StartServers(MyPeerData self)
        {
            try
            {
                //Start all servers in separate threads
                self.serverAddresses.ForEach(address => new Thread(o => this.Server(self, address.port)).Start());
                Console.ForegroundColor = ConsoleColor.DarkGray;
                self.serverAddresses.ForEach(address => Console.WriteLine("Started serving on {0}:{1}", "127.0.0.1", address.port));
                Console.ForegroundColor = ConsoleColor.White;
                //Servers must be running before clients may connect
                //Thread.Sleep(1000);

                //Create all tcpClients
                //self.tcpClientAddresses.ForEach(address => self.tcpClients.Add(new TcpClient(address.address, address.port)));
                //Start one threads that manages the connection to all communication partners
                //new Thread(o => this.Client(self)).Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
            }

        }


        public bool Join(MyPeerData self, IP OriginIp, IP IpToJoin)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Join {0}", self.myPeerID);
            Console.ForegroundColor = ConsoleColor.White;
            Message newAnswerMessage = new Message
            {
                Type = Message.Types.JoinRequest,
                DestinationIP = IpToJoin,
                SourceIP = OriginIp,
                SourceId = self.myPeerID,
                Ttl = 5,
                ChatMessage = "JoinInit"
            };

            string message = FillSpace(newAnswerMessage);

            Byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                TcpClient tcpClient = new TcpClient(newAnswerMessage.DestinationIP.address, newAnswerMessage.DestinationIP.port);
                //Console.WriteLine("Method: {0}, new TcpClient {1}:{2}", "Join", newAnswerMessage.DestinationIP.address, newAnswerMessage.DestinationIP.port);

                if (!tcpClient.Connected)
                {
                    var c = tcpClient.Client;
                    tcpClient.ConnectAsync(LocalAddr, ((IPEndPoint)c.RemoteEndPoint).Port);
                }

                NetworkStream stream = tcpClient.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                // Receive the TcpServer.response.

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes. 
                try
                {
                    Int32 bytes = stream.Read(data, 0, data.Length);
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(ex);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, data.Length);

              //  Console.WriteLine("Received: {0}", responseData.TrimEnd('-'));

                try
                {
                    Message messageObj = JsonConvert.DeserializeObject<Message>(responseData.TrimEnd('-'));
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\n");
                    Console.WriteLine("Send from : {0}",messageObj.SourceId);
                    Console.WriteLine("Send at : '{0}'", messageObj.TimeStamp);
                    Console.WriteLine("Message : '{0}'", messageObj.ChatMessage);
                    Console.ForegroundColor = ConsoleColor.White;

                    ProzessNachricht(self, stream, responseData, messageObj, tcpClient);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Could not deserialize malformed message. Message was {0}. Failed with {1}", data, ex.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }
                return true;

                // Close everything.
                //stream.Close();
                //tcpClient.Close();
                //Console.WriteLine("Disconnected");
            }
            catch (ArgumentNullException e)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("ArgumentNullException: {0}", e);
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            catch (SocketException e)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Could not establish a connection to known stable peer {0}:{1}", IpToJoin.address, IpToJoin.port);
                Console.ForegroundColor = ConsoleColor.White;
                //Console.WriteLine("SocketException: {0}", e);
                return false;
            }
            catch (System.IO.IOException)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Called Join after quit");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
        }


        public void Client(MyPeerData self)
        {
            string author = string.Empty;

            while (go_on)
            {
                Thread.Sleep(300);
                /* if (self.Authorgiven == false)
                 {
                     Console.WriteLine("Give yourself a Name : ");


                     author = Console.ReadLine();
                     self.Authorgiven = true;
                 }
                */
                // Console.WriteLine("Enter your Message: ");
                string line = Console.ReadLine();


                if (line.Length != 0)
                {
                    Message messageObj = new Message();
                    if (!line.Equals("quit"))
                    {
                        messageObj = new Message
                        {
                            Type = Message.Types.PersonalMessage,
                            ChatMessage = line,
                            SourceId = self.myPeerID
                            //AuthorName = author
                        };
                    }

                    if (line.Equals("quit"))
                    {
                        messageObj = new Message
                        {
                            Type = Message.Types.KillConnection,
                            ChatMessage = line,
                            SourceId = self.myPeerID
                            //AuthorName = author
                        };

                    }

                    string message = FillSpace(messageObj);

                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                    foreach (var client in self.tcpClients)
                    {
                        // Console.WriteLine("Method: {0}, client on port {1}", "Client", client.Client.RemoteEndPoint.ToString());
                        try
                        {
                            if (!client.Value.Connected)
                            {
                                var c = client.Value.Client;
                                c.ConnectAsync(LocalAddr, ((IPEndPoint)c.RemoteEndPoint).Port);
                            }

                            NetworkStream stream = client.Value.GetStream();

                            try
                            {
                                // Send the message to the connected TcpServer.
                                stream.Write(data, 0, data.Length);

                                // Receive the TcpServer.response.

                                // String to store the response ASCII representation.
                                String responseData = String.Empty;

                                //// Read the first batch of the TcpServer response bytes.
                                //Int32 bytes = stream.Read(data, 0, data.Length);
                                //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);





                                if (line.Equals("quit"))
                                {
                                    go_on = false;
                                    // Close everything.
                                    System.Threading.Thread.Sleep(2000);
                                    stream.Close();
                                    client.Value.Close();
                                    Console.WriteLine("Disconnected ");
                                }

                            }
                            catch (Exception ex)
                            {
                                self.tcpClients.Remove(client);
                            }
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


        public void Server(MyPeerData self, Int32 port)
        {
           // Console.WriteLine("Opened server on port {0}", port);
            TcpListener server = null;
            try
            {
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(LocalAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[MESSAGE_MAX_LENGTH];
                String data = null;

                // Enter the listening loop.
                while (true)
                {

                  //  Thread.Sleep(1000);

                 //   Console.WriteLine("Waiting for a connection...");
                   // Console.WriteLine("\n");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    Thread.Sleep(1000);

                    TcpClient client = server.AcceptTcpClient();
                   // Console.WriteLine("Connected!"); 
                   // Console.WriteLine("\n");


                    data = null;


                    if (!client.Connected)
                    {
                        var c = client.Client;
                        client.ConnectAsync(LocalAddr, ((IPEndPoint)c.RemoteEndPoint).Port);
                    }

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    try
                    {
                        // Loop to receive all the data sent by the client.
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            //Console.WriteLine("You Got a Message with this Header: {0}", data.TrimEnd('-'));

                            try
                            {
                                Message message = JsonConvert.DeserializeObject<Message>(data.TrimEnd('-'));

                                Console.WriteLine("\n");
                                string s = String.Format("Friend {0} at {1}", message.SourceId, message.TimeStamp.ToShortTimeString());
                                Console.CursorLeft = Console.BufferWidth - s.Length;
                                Console.Write(s);

                                s = String.Format("{0}", message.ChatMessage);
                                Console.CursorLeft = Console.BufferWidth - s.Length;
                                Console.Write(s);

                                //Console.WriteLine(data.TrimEnd('-'));
                                Console.Write("\n");
                                Console.CursorLeft = 0;
                                ProzessNachricht(self, stream, data, message, client);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Could not deserialize malformed message. Message was {0}. Failed with {1}", data, ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        stream.Close();
                        client.Close();
                    }

                    // Shutdown and end connection
                    //client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("SocketException: {0}", e);
                Console.WriteLine("\nFailed to create server on port {0}", port);
                Console.ForegroundColor = ConsoleColor.White;
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nHit enter to continue...");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Read();
        }

        /*Hier passiert die Logic eines Peers. Hier steht was er bei welchem Nachrichtentyp Macht etc*/
        private void ProzessNachricht(MyPeerData self, NetworkStream stream, string data, Message message, TcpClient tcpClient)
        {


            //// Haben wir diese Nachricht schonmal gesehen
            //if (self.seenMessages.Any(x => x.TimeStamp == message.TimeStamp && x.SourceId == message.SourceId))
            //{
            //    return;
            //}
            //else
            //{
            //    self.seenMessages.Add(message);
            //    if (self.seenMessages.Count > 1500)
            //    {
            //        self.seenMessages.RemoveRange(0, 500);
            //    }
            //}


            switch (message.Type)
            {

                case Message.Types.PersonalMessage:
                    //client will handle it automatically
                    break;
                case Message.Types.GroupMessage:
                    //TODO
                    break;

                case Message.Types.WannabeNeighbour:
                    //TODO 
                    break;

                //Jessies Cases
                case Message.Types.JoinRequest:
                    OnPeerJoinRequest(self, message, stream);
                    break;
                case Message.Types.JoinResponse:
                    OnPeerJoinResponse(self, message, stream, tcpClient);
                    break;
                case Message.Types.JoinAcknowledge:
                    OnPeerJoinAcknowledge(self, message, stream, tcpClient);
                    break;
                case Message.Types.KillConnection:
                    OnPeerKillConnection(self, message, stream, tcpClient);
                    break;
                default:
                    Console.WriteLine("Could not handle message of unknown type {0}. Message was {1}", message.Type, message.ToJson());
                    break;
            }
        }


        public void OnPeerJoinRequest(MyPeerData self, Message incommingMessage, NetworkStream stream)
        {

            //Console.WriteLine("OnPeerJoinRequest {0}", self.myPeerID);
          
            Thread.Sleep(1000);

            if (self.tcpClientAddresses.Any(x => x.Key == incommingMessage.SourceId))
            {
                return;
            }

            Message newAnswerMessage = new Message
            {
                Type = Message.Types.JoinResponse,
                DestinationIP = incommingMessage.SourceIP,
                SourceIP = self.GetNextFreePort(),
                ChatMessage = "JoinRequest",
                SourceId = self.myPeerID,
                DestinationId = incommingMessage.SourceId
            };

            ////if peer has no neighbors or TTL == 0, it'll transmits its own address
            //if (self.tcpClientAddresses.Count == 0 || incommingMessage.Ttl == 0)
            //{
            string send = FillSpace(newAnswerMessage);

            Byte[] sendableMessage = System.Text.Encoding.ASCII.GetBytes(send);

            // Send back a response.
            stream.Write(sendableMessage, 0, sendableMessage.Length);
            string data = System.Text.Encoding.ASCII.GetString(sendableMessage, 0, sendableMessage.Length);
            
           // Console.WriteLine("Sent: {0}", data.TrimEnd('-'));

            StartServer(self, newAnswerMessage.SourceIP, newAnswerMessage.DestinationId);

            //}
            //else
            //{
            //    // select one known neighbor and send it back
            //    // ...
            //    if (incommingMessage.Ttl > 0)
            //    {
            //        incommingMessage.Ttl--;
            //        // todo does the clients neighbor have an open port to speak with us?
            //        var nextRandomWalkStep = self.tcpClientAddresses.ElementAt(random.Next(self.tcpClientAddresses.Count));
            //        incommingMessage.DestinationIP = nextRandomWalkStep;
            //        OnPeerJoinRequest(self, incommingMessage, stream);
            //    }
            //}            
        }

        private void OnPeerJoinResponse(MyPeerData self, Message message, NetworkStream stream, TcpClient tcpClient)
        {

            //Console.WriteLine("OnPeerJoinResponse {0}", self.myPeerID);

            StartServer(self, message.DestinationIP, message.SourceId);

            Thread.Sleep(1000);

            StartTcpClient(self, message.SourceIP, message.SourceId);

            Message newAnswerMessage = new Message
            {
                Type = Message.Types.JoinAcknowledge,
                DestinationIP = message.SourceIP,
                SourceIP = message.DestinationIP,
                ChatMessage = "JoinResponse",
                SourceId = self.myPeerID
            };

            string send = FillSpace(newAnswerMessage);

            Byte[] sendableMessage = System.Text.Encoding.ASCII.GetBytes(send);

            // Send back a response.
            stream.Write(sendableMessage, 0, sendableMessage.Length);
            string data = System.Text.Encoding.ASCII.GetString(sendableMessage, 0, sendableMessage.Length);

          //  Console.WriteLine("Acknowledge: {0}", data.TrimEnd('-'));

            //stream.Close(); //todo
            //tcpClient.Close();
        }

        private void OnPeerJoinAcknowledge(MyPeerData self, Message message, NetworkStream stream, TcpClient tcpClient)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("OnPeerJoinAcknowledge {0}", self.myPeerID);
            Console.ForegroundColor = ConsoleColor.White;
            Thread.Sleep(1000);

            Message messagea = new Message
            {
                SourceId = self.myPeerID,
                ChatMessage = "JoinAcknowledge",
                DestinationId = message.SourceId
            };

            Console.WriteLine("\n");
            Console.WriteLine("Source / You : '{0}'", messagea.SourceId);
            Console.WriteLine("Message : '{0}'", messagea.ChatMessage);
            Console.WriteLine("Destination : '{0}'", messagea.DestinationId);
            

            //self.ownAdresses.Add(new KeyValuePair<int, IP>(message.SourceId, message.DestinationIP));
            StartTcpClient(self, message.SourceIP, message.SourceId);
            stream.Close();
            tcpClient.Close();
        }

        private void OnPeerKillConnection(MyPeerData self, Message message, NetworkStream stream, TcpClient tcpClient)
        {

            var ports = self.ownAdresses.Where(x => x.Key == message.SourceId).Select(y => y.Value.port).ToList();

            foreach(var port in ports)
            {
                self.tcpClients.RemoveAll(x => x.Key == message.SourceId);
            }


            //lösche tcpClient der den port hat, den laut ownAdressesliste der peer hat, der quittet
            self.serverAddresses.RemoveAll(x => (x.port == ((IPEndPoint)tcpClient.Client.LocalEndPoint).Port));

            stream.Close();
            tcpClient.Close();
        }


        private void StartServer(MyPeerData self, IP partnerIp, int partnerId)
        {
            self.ownAdresses.Add(new KeyValuePair<int, IP>(partnerId, partnerIp));
            self.serverAddresses.Add(partnerIp);
            new Thread(o => this.Server(self, partnerIp.port)).Start();

            //Console.WriteLine("Started server on {0}:{1}", partnerIp.address, partnerIp.port);
        }

        private void StartTcpClient(MyPeerData self, IP otherPeerIp, int otherPeerId)
        {
            if (!self.tcpClientAddresses.Any(x => x.Key == otherPeerId))
            {
                self.tcpClients.Add(new KeyValuePair<int, TcpClient>(otherPeerId, new TcpClient(otherPeerIp.address, otherPeerIp.port)));
                self.tcpClientAddresses.Add(new KeyValuePair<int, IP>(otherPeerId, new IP(otherPeerIp.address, otherPeerIp.port)));
                new Thread(o => this.Client(self)).Start();
                //Console.WriteLine("Started TCP client on {0}:{1}", myIp.address, myIp.port);
            }
        }


        public bool ManageJoin(MyPeerData self)
        {
            Random random = new Random();
            //overlay mode
            //var selectedStablePeer = self.knownStablePeers.ElementAt(random.Next(self.knownStablePeers.Count));

            //bool value = false; // Used to store the return value
            //var thread = new Thread(() =>
            //{
            //    value = Join(self, self.GetNextFreePort(), selectedStablePeer);
            //});
            //thread.Start();
            //thread.Join();
            //Console.WriteLine(value);
            //return value;

            //groupchat mode
            bool value = true; // Used to store the return value
            foreach (var stablePeer in self.knownStablePeers)
            {
                var thread = new Thread(() =>
                {
                    value = value && Join(self, self.GetNextFreePort(), stablePeer);
                });
                thread.Start();
                thread.Join();
                if (value)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Succeeded - Connection to peer at {0}:{1}", stablePeer.address, stablePeer.port);
                    //self.tcpClientAddresses.Add(new IP(stablePeer.address, stablePeer.port));
                    Console.WriteLine("\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Failed - Connection to peer at {0}:{1}", stablePeer.address, stablePeer.port);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            if (self.knownStablePeers.Count == 0) { value = false; }
            return value;

        }


        #region Other Functions #region Allerlei Funktionen

        private Boolean WillIBecomeANewNeighbour(MyPeerData self)
        {

            Random rand1 = new Random();
            if (self.tcpClients.Count < 3)
            {
                return true;
            }
            else
            {
                return rand1.NextDouble() < (1 / self.tcpClients.Count);
            }
        }

        /// <summary>
        /// stream readers can only process streams of known length
        /// </summary>
        private string FillSpace(Message message)
        {
            string send = message.ToJson();
            return send.PadRight(MESSAGE_MAX_LENGTH, '-');
        }

        #endregion

    }
}
