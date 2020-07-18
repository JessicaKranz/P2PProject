using Datenmodelle;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace BusinessLogic
{
    public class TcpConnection
    {
        const int MESSAGE_MAX_LENGTH = 512;
        readonly IPAddress LocalAddr = IPAddress.Parse("127.0.0.1");
        Random random = new Random();
        bool go_on = true;
        const int TTL_MAX = 1;

       
        public bool Join(MyPeerData self, IP OriginIp, IP IpToJoin)
        {
            Console.WriteLine("Join {0}", self.myPeerID);
            Message newAnswerMessage = new Message
            {
                Type = Message.Types.JoinRequest,
                DestinationIP = IpToJoin,
                SourceIP = OriginIp,
                SourceId = self.myPeerID,
                Ttl = TTL_MAX,
                ChatMessage = "JoinInit",
                JoiningId = self.myPeerID
            };

            try
            {
                StartServer(self, OriginIp, 0);
            }
            catch (SocketException)
            {
                Console.WriteLine("Server already running");
            }
            

            string message = FillSpace(newAnswerMessage);

            Byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                TcpClient tcpClient = new TcpClient(newAnswerMessage.DestinationIP.address, newAnswerMessage.DestinationIP.port);

                //Console.WriteLine("Method: {0}, new TcpClient {1}:{2}", "Join", newAnswerMessage.DestinationIP.address, newAnswerMessage.DestinationIP.port);

                //if (!tcpClient.Connected)
                //{
                //    var c = tcpClient.Client;
                //    tcpClient.ConnectAsync(LocalAddr, ((IPEndPoint)c.RemoteEndPoint).Port);
                //}

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
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                responseData = Encoding.ASCII.GetString(data, 0, data.Length);

                //  Console.WriteLine("Received: {0}", responseData.TrimEnd('-'));

                try
                {
                    Message messageObj = JsonConvert.DeserializeObject<Message>(responseData.TrimEnd('-'));

                    Console.WriteLine("\n");
                    Console.WriteLine("Send from : {0}", messageObj.SourceId);
                    Console.WriteLine("Send at : '{0}'", messageObj.TimeStamp);
                    Console.WriteLine("Message : '{0}'", messageObj.ChatMessage);

                    ProzessNachricht(self, stream, responseData, messageObj, tcpClient);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not deserialize malformed message. Message was {0}. Failed with {1}", data, ex.Message);
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
                Console.WriteLine("ArgumentNullException: {0}", e);
                return false;
            }
            catch (SocketException e)
            {
                Console.WriteLine("Could not establish a connection to known stable peer {0}:{1}", IpToJoin.address, IpToJoin.port);
                //Console.WriteLine("SocketException: {0}", e);
                return false;
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Called Join after quit");
                return false;
            }
        }


        public void Client(MyPeerData self)
        {
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
                            SourceId = self.myPeerID,
                            JoiningId = self.myPeerID,
                            Ttl = 1
                        };
                    }

                    if (line.Equals("quit"))
                    {
                        messageObj = new Message
                        {
                            Type = Message.Types.KillConnection,
                            ChatMessage = line,
                            SourceId = self.myPeerID,
                            JoiningId = self.myPeerID,
                            Ttl = 1
                        };

                    }

                    string message = FillSpace(messageObj);

                    Byte[] data = Encoding.ASCII.GetBytes(message);

                    try
                    {
                        foreach (var client in self.tcpClients)
                        {
                            // Console.WriteLine("Method: {0}, client on port {1}", "Client", client.Client.RemoteEndPoint.ToString());
                            try
                            {
                                //if (!client.Value.Connected)
                                //{
                                //    var c = client.Value.Client;
                                //    c.ConnectAsync(LocalAddr, ((IPEndPoint)c.RemoteEndPoint).Port);
                                //}

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
                                        Thread.Sleep(2000);
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
                    }catch(Exception ex)
                    {
                        Console.WriteLine("Lost peer");
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
                server.ExclusiveAddressUse = false;

                // Start listening for client requests.
                server.Start();

                self.tcpListener.Add(server);

                Console.WriteLine("Started peer at port {0}", port);

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
                    TcpClient client = server.AcceptTcpClient();
                    // Console.WriteLine("Connected!"); 
                    // Console.WriteLine("\n");


                    data = null;


                    if (!client.Connected)
                    {
                        var c = client.Client;
                        client.ConnectAsync(LocalAddr, ((IPEndPoint)c.RemoteEndPoint).Port);
                    }

                    Thread.Sleep(1000);

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
                                Console.WriteLine("Send from : {0}", message.SourceId);
                                Console.WriteLine("Send at : '{0}'", message.TimeStamp);
                                Console.WriteLine("Message : '{0}'", message.ChatMessage);
                                //Console.WriteLine(data.TrimEnd('-'));
                                //Console.WriteLine("\n");
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
                        Console.WriteLine(ex);
                        //return;
                    }


                    // Shutdown and end connection
                    //client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                Console.WriteLine("\nFailed to create server on port {0}", port);
            }
            finally
            {
                // Stop listening for new clients.
                //server.Stop();
            }

            return;
            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        /*Hier passiert die Logic eines Peers. Hier steht was er bei welchem Nachrichtentyp Macht etc*/
        private void ProzessNachricht(MyPeerData self, NetworkStream stream, string data, Message message, TcpClient tcpClient)
        {
            switch (message.Type)
            {
                case Message.Types.PersonalMessage:
                    OnPersonalMessage(self, message, stream);
                    break;
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

        public void OnPersonalMessage(MyPeerData self, Message incommingMessage, NetworkStream stream)
        {
            //broadcast message to all contacts that were not the original sender
            if(incommingMessage.Ttl > 0)
            {
                Message answer = incommingMessage;
                answer.Ttl--;
                var peersToSendMessageTo = self.tcpClients.Where(x => x.Key != incommingMessage.JoiningId).ToList();

                foreach (var peer in peersToSendMessageTo)
                {

                    stream = peer.Value.GetStream();

                    string send = FillSpace(answer);

                    Byte[] sendableMessage = System.Text.Encoding.ASCII.GetBytes(send);

                    stream.Write(sendableMessage, 0, sendableMessage.Length);
                    string data = System.Text.Encoding.ASCII.GetString(sendableMessage, 0, sendableMessage.Length);
                }            
            }                      
        }


        public void OnPeerJoinRequest(MyPeerData self, Message incommingMessage, NetworkStream stream)
        {

            //Console.WriteLine("OnPeerJoinRequest {0}", self.myPeerID);

            Thread.Sleep(1000);

            if (self.tcpClientAddresses.Any(x => x.Key == incommingMessage.SourceId && x.Key != 0))
            {
                //return;
            }

            Message answer = new Message();

            //if peer has no neighbors or TTL == 0, it'll transmits its own address
            if (self.tcpClientAddresses.Count == 0 || incommingMessage.Ttl == 0)
            {
                answer = new Message
                {
                    Type = Message.Types.JoinResponse,
                    DestinationIP = incommingMessage.SourceIP,
                    SourceIP = self.GetNextFreePort(),
                    ChatMessage = "JoinRequest",
                    SourceId = self.myPeerID,
                    DestinationId = incommingMessage.SourceId,
                    JoiningId = incommingMessage.JoiningId
                };            

                if(incommingMessage.Ttl == 0 && !self.tcpClientAddresses.Any(x => x.Key == answer.DestinationId))
                {
                    Thread createClient = new Thread(o => StartTcpClient(self, new IP(LocalAddr.ToString(), incommingMessage.SourceIP.port), incommingMessage.SourceId));
                    createClient.Start();
                    createClient.Join();
                }
                    
                 
                if (self.tcpClientAddresses.Count > 0 && self.tcpClients.Any(x => x.Key == answer.DestinationId))
                {
                    TcpClient tcpClient = self.tcpClients.Where(x => x.Key == answer.DestinationId).FirstOrDefault().Value;
                    stream = tcpClient.GetStream();
                }
                incommingMessage.Ttl--;


            }
            else
            {
                // select one known neighbor and send it back
                // ...
                if (incommingMessage.Ttl > 0)
                {
                    //entry peer
                    if(incommingMessage.Ttl == TTL_MAX)
                    {
                        var s = incommingMessage.SourceIP.port;
                        int f = (int)Math.Floor(s / 100d) * 100;
                        StartTcpClient(self, new IP(LocalAddr.ToString(), f), incommingMessage.SourceId);
                        //self.tcpClientAddresses.Add(new KeyValuePair<int, IP>(incommingMessage.SourceId, new IP(incommingMessage.SourceIP.address, incommingMessage.SourceIP.port)));
                    }
                    incommingMessage.Ttl--;
                    //todo doesn't work for greater ttl's
                    //must use variable like joiningId
                    var listOfPeersWithoutJoiningPeer = self.tcpClientAddresses.Where(x => x.Key != incommingMessage.JoiningId).ToList();
                    var nextRandomWalkStep = listOfPeersWithoutJoiningPeer.ElementAt(random.Next(listOfPeersWithoutJoiningPeer.Count));
                    var i = nextRandomWalkStep.Value.port;
                    int o = (int)Math.Floor(i / 100d) * 100;
                    incommingMessage.DestinationIP = new IP(LocalAddr.ToString(), o);

                    
                    answer = incommingMessage;

                    //send bootstrap message to selected peer
                    try
                    {
                        TcpClient tcpClient = self.tcpClients.Where(x => x.Key == nextRandomWalkStep.Key).FirstOrDefault().Value;
                        try
                        {
                            tcpClient.ConnectAsync(LocalAddr, ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port);

                        }catch(Exception ex)
                        {
                            Console.WriteLine("Doppelter Verbindungsaufbau");
                        }
                        stream = tcpClient.GetStream();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Broke up while adding new peer to network");
                        Console.WriteLine(ex.Message);
                    }                   
                }
            }


            if (self.tcpClientAddresses.Count == 0 || incommingMessage.Ttl < 0) 
                StartServer(self, answer.SourceIP, answer.DestinationId);

            string send = FillSpace(answer);

            Byte[] sendableMessage = System.Text.Encoding.ASCII.GetBytes(send);

            stream.Write(sendableMessage, 0, sendableMessage.Length);
            string data = System.Text.Encoding.ASCII.GetString(sendableMessage, 0, sendableMessage.Length);
        }


        private void OnPeerJoinResponse(MyPeerData self, Message message, NetworkStream stream, TcpClient tcpClient)
        {

            //Console.WriteLine("OnPeerJoinResponse {0}", self.myPeerID);

            try
            {
                StartServer(self, message.DestinationIP, message.SourceId);
            }
            catch (SocketException)
            {
                Console.WriteLine("Server already running");
            }

            Thread.Sleep(1000);

            StartTcpClient(self, message.SourceIP, message.SourceId);

            Message newAnswerMessage = new Message
            {
                Type = Message.Types.JoinAcknowledge,
                DestinationIP = message.SourceIP,
                SourceIP = message.DestinationIP,
                ChatMessage = "JoinResponse",
                SourceId = self.myPeerID,
                JoiningId = message.JoiningId
            };

            string send = FillSpace(newAnswerMessage);

            Byte[] sendableMessage = System.Text.Encoding.ASCII.GetBytes(send);

            // Send back a response.
            stream = self.tcpClients.Where(x => x.Key == message.SourceId).FirstOrDefault().Value.GetStream();
            stream.Write(sendableMessage, 0, sendableMessage.Length);
            string data = System.Text.Encoding.ASCII.GetString(sendableMessage, 0, sendableMessage.Length);

            //  Console.WriteLine("Acknowledge: {0}", data.TrimEnd('-'));

            var closedEndPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint;

            tcpClient.GetStream().Close();
            tcpClient.Client.Close();
            tcpClient.Close();
            //stream.Dispose();
            tcpClient.Dispose();
            Thread.Sleep(1000);



            //var p = self.tcpListener.Where(x => ((IPEndPoint)x.LocalEndpoint).Port == self.requestAddress.port).ToList();
            //p.FirstOrDefault().Stop();
            //Thread.Sleep(1000);
            //p.FirstOrDefault().Start();


            //StartServer(self, self.requestAddress, 0);
            //Thread.Sleep(1000);

        }

        private void OnPeerJoinAcknowledge(MyPeerData self, Message message, NetworkStream stream, TcpClient tcpClient)
        {
            Console.WriteLine("OnPeerJoinAcknowledge {0}", self.myPeerID);
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

            //tcpClient.GetStream().Close();
            //tcpClient.Client.Close();
            //tcpClient.Close();
            //stream.Dispose();
            //tcpClient.Dispose();


            Thread.Sleep(1000);
            //var p = self.tcpListener.Where(x => ((IPEndPoint)x.LocalEndpoint).Port == self.requestAddress.port).ToList();
            //p.FirstOrDefault().Server.Close();
            //p.FirstOrDefault().Stop();
            //Thread.Sleep(1000);
            ////p.FirstOrDefault().Start();
            //StartServer(self, self.requestAddress, 0);
            //Thread.Sleep(1000);


        }

        private void OnPeerKillConnection(MyPeerData self, Message message, NetworkStream stream, TcpClient tcpClient)
        {
            Console.WriteLine("\nOwnAddresses");
            self.ownAdresses.ForEach(x => Console.WriteLine(x.Key + " " + x.Value.address + " " + x.Value.port));
            Console.WriteLine("\nServerAddresses");
            self.serverAddresses.ForEach(x => Console.WriteLine(x.address + " " + x.port));
            Console.WriteLine("\nTcpClientAddresses");
            self.tcpClientAddresses.ForEach(x => Console.WriteLine(x.Key + " " + x.Value.address + " " + x.Value.port));



            var ports = self.ownAdresses.Where(x => x.Key == message.SourceId).Select(y => y.Value.port).ToList();

            foreach (var port in ports)
            {
                self.tcpClients.RemoveAll(x => x.Key == message.SourceId);
            }


            //lösche tcpClient der den port hat, den laut ownAdressesliste der peer hat, der quittet
            self.serverAddresses.RemoveAll(x => (x.port == ((IPEndPoint)tcpClient.Client.LocalEndPoint).Port));

            stream.Close();
            tcpClient.Close();

            Console.WriteLine("\nOwnAddresses");
            self.ownAdresses.ForEach(x => Console.WriteLine(x.Key + " " + x.Value.address + " " + x.Value.port));
            Console.WriteLine("\nServerAddresses");
            self.serverAddresses.ForEach(x => Console.WriteLine(x.address + " " + x.port));
            Console.WriteLine("\nTcpClientAddresses");
            self.tcpClientAddresses.ForEach(x => Console.WriteLine(x.Key + " " + x.Value.address + " " + x.Value.port));

        }

        public void StartServers(MyPeerData self)
        {
            try
            {
                //Start all servers in separate threads
                self.serverAddresses.ForEach(address => new Thread(o => this.Server(self, address.port)).Start());
                self.serverAddresses.ForEach(address => Console.WriteLine("Started serving on {0}:{1}", "127.0.0.1", address.port));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private void StartServer(MyPeerData self, IP partnerIp, int partnerId)
        {
            self.ownAdresses.Add(new KeyValuePair<int, IP>(partnerId, partnerIp));
            self.serverAddresses.Add(partnerIp);
            new Thread(o => this.Server(self, partnerIp.port)).Start();

            Console.WriteLine("Started server on {0}:{1}", partnerIp.address, partnerIp.port);
        }

        private void StartTcpClient(MyPeerData self, IP otherPeerIp, int otherPeerId)
        {
            
            TcpClient tcpClient = new TcpClient(otherPeerIp.address, otherPeerIp.port);
            try
            {
                tcpClient.ConnectAsync(LocalAddr, otherPeerIp.port);

            }catch(Exception)
            {
                Console.WriteLine("Doppelter Verbindungsaufbau");
            }


                self.tcpClients.Add(new KeyValuePair<int, TcpClient>(otherPeerId, tcpClient));
                self.tcpClientAddresses.Add(new KeyValuePair<int, IP>(otherPeerId, new IP(otherPeerIp.address, otherPeerIp.port)));
                new Thread(o => this.Client(self)).Start();
                //Console.WriteLine("Started TCP client on {0}:{1}", myIp.address, myIp.port);
            //}
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

                    Console.WriteLine("Succeeded - Connection to peer at {0}:{1}", stablePeer.address, stablePeer.port);
                    //self.tcpClientAddresses.Add(new IP(stablePeer.address, stablePeer.port));
                    Console.WriteLine("\n");
                }
                else
                {
                    Console.WriteLine("Failed - Connection to peer at {0}:{1}", stablePeer.address, stablePeer.port);
                }
            }
            if (self.knownStablePeers.Count == 0) { value = false; }
            return value;

        }


        /// <summary>
        /// stream readers can only process streams of known length
        /// </summary>
        private string FillSpace(Message message)
        {
            string send = message.ToJson();
            return send.PadRight(MESSAGE_MAX_LENGTH, '-');
        }
    }
}
