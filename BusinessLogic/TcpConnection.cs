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

namespace BusinessLogic
{
    public class TcpConnection
    {
        const int MESSAGE_MAX_LENGTH = 2048;

        List<TcpClient> tcpClients = new List<TcpClient>();

        bool go_on = true;
        Random random = new Random();

        public void StartServersAndClients(MyPeerData self)
        {
            //Start all servers in separate threads
            self.serverAddresses.ForEach(address => new Thread(o => this.Server(self, address.port)).Start());
            self.serverAddresses.ForEach(address => Console.WriteLine("Started serving on {0}:{1}", "127.0.0.1", address.port));

            //Servers must be running before clients may connect
            Thread.Sleep(1000);

            //Create all tcpClients
            self.tcpClientAddresses.ForEach(address => tcpClients.Add(new TcpClient(address.address, address.port)));
            //Start one threads that manages the connection to all communication partners
            new Thread(o => this.Client(tcpClients)).Start();
        }


        public void Join(MyPeerData self, IP OriginIp, IP IpToJoin)
        {
            Message messageData = new Message
            {
                Type = Message.Types.JoinRequest,
                Destination = IpToJoin,
                Source = OriginIp,
                SourceId = self.myPeerID,
                Ttl = 5
            };

            //stream readers can only process streams of known length
            string message = messageData.ToJson();
            message = message.PadRight(MESSAGE_MAX_LENGTH, '-');

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
                    line = line.PadRight(MESSAGE_MAX_LENGTH, ' ');

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



        public void Server(MyPeerData self, Int32 port)
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
                Byte[] bytes = new Byte[MESSAGE_MAX_LENGTH];
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
                            Message message = JsonConvert.DeserializeObject<Message>(data.TrimEnd('-'));
                            ProzessNachricht(self, stream,data, message);
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

        public void OnPeerJoinRequest(MyPeerData self, Message message, NetworkStream stream)
        {
            Message messageData = new Message
            {
                Type = Message.Types.JoinResponse,
                Destination = message.Source,
                Source = self.GetNextFreePort()
            };

            //if peer has no neighbors, it'll transmits its own address
            if (self.tcpClientAddresses.Count == 0)
            {
                //stream readers can only process streams of known length
                string send = messageData.ToJson();
                send = send.PadRight(MESSAGE_MAX_LENGTH, '-');

                Byte[] msg = System.Text.Encoding.ASCII.GetBytes(send);

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
                    Join(self, message.Source, nextRandomWalkStep);
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




        #region EingehendeNachrichtenLogik 
        /*Hier passiert die Logic eines Peers. Hier steht was er bei welchem Nachrichtentyp Macht etc*/
        private void ProzessNachricht(MyPeerData self, NetworkStream stream, string data, Message n)
        {
           

            //TODO entweder hier oder woanders: Check ob man diese Nachricht schonmal gesehn hat über n.Timestamp und n.origin, n.destination. Man braucht dafür ne Liste.

            // Fügt bisher ungesehene Peers einer Liste hinzu
            
            if (!self.bekanntePeers.Any(x => x.peerID == n.SourceId && x.associatedName == n.AuthorName))  
            {
                self.bekanntePeers.Add(new Peer(n.SourceId, n.AuthorName));
            }
            else if (null != self.bekanntePeers.Where(x => x.peerID == n.SourceId))
            {
                self.bekanntePeers.Find(x => x.peerID == n.SourceId).associatedName = n.AuthorName;
                self.bekanntePeers.Find(x => x.peerID == n.SourceId && x.associatedName == n.AuthorName).UpdateLastSeen();
            }
            else
            {
                self.bekanntePeers.Find(x => x.peerID == n.SourceId && x.associatedName == n.AuthorName).UpdateLastSeen();
            }
            
            switch (n.Type)
            {
                //Leos Cases
                case Message.Types.PeerEntry:
                    IncommingPeerEntryMessage(self , n);
                    break;
                case Message.Types.PeerJoin:
                    IncommingPeerJoinMessage(self, n);
                    break;
                case Message.Types.PersonalMessage:
                    IncommingPersonalMessage(self, n);
                    break;
                case Message.Types.GroupMessage:
                    IncommingGroupMessage(self, n);
                    break;
                case Message.Types.FishTank:
                    IncommingFishTankMessage(self, n);
                    break;
                case Message.Types.WannabeNeighbour:
                    //TODO dostuff()
                    break;

                //Jessies Cases
                case Message.Types.JoinRequest:
                    OnPeerJoinRequest(self, n, stream);
                    break;
                case Message.Types.JoinResponse:
                    OnPeerJoinResponse();
                    break;
                default:
                    OnChatMessageReceived(data, stream);
                    break;
            }
        }
        static void IncommingPeerEntryMessage(MyPeerData self, Message n)
        {
            n.Ttl = (int)(n.WishedNeighbours * 3 * (1 + 1 / self.myFish.GetPortion()));          // Erzeuge eine TTL die Zukksessiv heruntergesetzt wird, damit es im Overlay keine Geisternachrichten gibt.
            if (WillIBecomeANewNeighbour(self))
            {
                n.WishedNeighbours--;
                ConnectTCP(self, n.SendersIP, new Peer(n.SourceId, n.AuthorName));
            }
            if (n.WishedNeighbours > 0 && n.Ttl > 0)
            {
                SendMessageFloodOverlay(self, new Message
                {
                    Type = Message.Types.PeerJoin,
                    Ttl = n.Ttl,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    SendersIP = n.SendersIP,

                });
            }
        }

        static void IncommingPeerJoinMessage(MyPeerData self, Message n)
        {
            n.Ttl--;

            if (null == self.neighbours.Where(x => x.partnerIPAddress == n.SendersIP))
            {
                if (WillIBecomeANewNeighbour(self))
                {
                    n.WishedNeighbours--;
                    ConnectTCP(self, n.SendersIP, new Peer(n.SourceId, n.AuthorName));
                }
            }

            if (n.WishedNeighbours > 0)
            {
                SendMessageFloodOverlay(self, new Message
                {
                    Type = Message.Types.PeerJoin,
                    Ttl = n.Ttl,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    SendersIP = n.SendersIP,

                });
            }
        }

        static void IncommingPersonalMessage(MyPeerData self, Message n)
        {
            n.Ttl--;
            if (n.DestinationId == self.myPeerID)
            {
                //Maybe Find a better way to deliver
                Console.WriteLine("[" + n.TimeStamp + n.AuthorName + "] " + " wrote to you:" + n.ChatMessage);
            }
            else if (null != self.neighbours.Where(x => x.partnerPeer.peerID == n.DestinationId))
            {
                SendMessage(self, n, self.neighbours.Where(x => x.partnerPeer.peerID == n.DestinationId).FirstOrDefault());
            }
            else
            {
                SendMessageFloodOverlay(self, n);
            }
        }

        static void IncommingGroupMessage(MyPeerData self, Message n)
        {
            n.Ttl--;
            if (null != self.myGroupChats.Find(x => x.groupChatID == n.DestinationId))
            {
                //Maybe Find a better way to deliver
                Console.WriteLine("[" + n.TimeStamp + n.AuthorName + "] " + " wrote in \"" + n.GroupChatName + "\":" + n.ChatMessage);
            }
            else
            {
                SendMessageFloodOverlay(self, n);
            }
        }

        //TODO REFACTOR THIS 
        static void IncommingFishTankMessage(MyPeerData self, Message n)
        {
            /*
            if (n.Fish.GetSize() > self.myFish.GetSize())
            {
                self.myFish = n.Fish;
                Reset(myFishTimer);
            }
            else if (n.Fish.GetSize() > myFish.GetSize())
            {
                myFish.SetPortion(myFish.GetPortion() + n.Fish.GetPortion());
                Reset(myFishTimer);
            }*/

        }

        #endregion

        #region Ausgehende Nachrichten Logik
        public static void SendPeerEntryMessage(MyPeerData self)
        {

            //TODO JESSICA NEEDS TO INSERT CODE HERE
            //FOR THIS WE DONT HAVE A CONNECTION YET? WHAT TO DO?

            //SendMessage(new Message { },new Connection )
        }

        private static void SendFTMessage(MyPeerData self )
        {
            self.myFish.SetPortion(self.myFish.GetPortion() / self.neighbours.Count() + 1); //Aktualisiere den eigenen Fish und versende Ihn. //TOTALK muss man das Synchronized machen?
            SendMessageFloodOverlay(self,new Message
            {
                Fish = self.myFish,
                SourceId = self.myPeerID,
                AuthorName = self.myName,
            });
        }

        public static void SendPersonalMessage(MyPeerData self, string PM, int destinationID)
        {
            SendMessageFloodOverlay(self, new Message
            {
                Type = Message.Types.PersonalMessage,
                Ttl = 7, //(int)(3 * (1 + 1 / myFish.GetPortion())), //TODO CHECK IF PMS REACH DESTINATION. OTHERWISE MAKE TTL HIGHER Maybe the given Algorithm will work
                AuthorName = self.myName,
                DestinationId = destinationID,
                SourceId = self.myPeerID,
                ChatMessage = PM,
            });
        }
        public static void SendGroupMessage(MyPeerData self, string GM, int destinationID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Versendet die Nachricht n an einen zufälligen Nachbarn im Overlay 
        /// </summary>
        /// <param name="n"></param>
        private static void SendMessageSingularRandom(MyPeerData self, Message n)
        {
            Random rand = new Random();
            //Maybe not send it back to previous Peer but not important
            var randomNeighbour = self.neighbours[rand.Next(self.neighbours.Count) - 1];
            SendMessage(self, n, randomNeighbour);
        }

        /// <summary>
        /// Versendet die Nachricht n an alle bekannten Nachbarn (Die Gesamte neigbours Liste)
        /// </summary>
        /// <param name="n"></param>
        private static void SendMessageFloodOverlay(MyPeerData self, Message n)
        {
            //Maybe not send it back to previous Peer but not important (n.SourceId tut nicht immer das gewünschte.
            foreach (Connection neighbour in self.neighbours)
            {
                SendMessage(self , n, neighbour);
            }
        }

        /// <summary>
        /// Sendet Nachricht n an den Peer mit dem er durch Connection c verbunden ist
        /// </summary>
        /// <param name="n"></param>
        /// <param name="c"></param>
        private static void SendMessage(MyPeerData self, Message n, Connection c)
        {
            //TODO //JESSICA DO STUFF HERE
        }

        #endregion

        #region Other Functions #region Allerlei Funktionen
       
        static void ConnectTCP(MyPeerData self, IPAddress ipAdresse, Peer newNeighbour)
        {
            //JESSICA DO STUFF HERE
            Console.WriteLine("Ich verbinde mich jetzt mit \"" + ipAdresse + "\"! (In wirklichkeit tue ich das noch nicht. Das kommt aber noch.");
            int neigboursPortHeUsesToTalkToMe = 666; // TODO OR TODELETE FROM CONNECTION DATENMODELL
            int myPortIUseToTalkWithHim = 666;       // TODO OR TODELETE FROM CONNECTION DATENMODELL
            self.neighbours.Add(new Connection
            {
                partnerPeer = newNeighbour,
                partnerIPAddress = ipAdresse,
                partnerPortNr = neigboursPortHeUsesToTalkToMe,
                myPortNr = myPortIUseToTalkWithHim
            });
            //TODO stelle verbindung mit dem Peer an folgender Ip.Adresse her. Fordere dafür alle informationen an die du brauchst. 
        }
        static Boolean WillIBecomeANewNeighbour(MyPeerData self)
        {
            return true; // TODO Should be like this below. Just delet stuff on the left.
            Random rand1 = new Random();
            return rand1.NextDouble() < self.myFish.GetPortion();
        }

        #endregion

        #region CalledEventsRegion
        private static void OnFishTimerEvent(MyPeerData self, Object source, ElapsedEventArgs e)
        {
            SendFTMessage(self);
        }
        public static void Reset(System.Timers.Timer timer)
        {
            timer.Stop();
            timer.Start();
        }


        //TODO LEONARD
        private static void SetFishTankTimer()
        {
            /*
            // Create a timer with a 60 second interval.
            myFishTimer = new System.Timers.Timer(60000);    // 1min
            // Hook up the Elapsed event for the timer. 
            myFishTimer.Elapsed += OnFishTimerEvent;
            myFishTimer.AutoReset = true;
            myFishTimer.Enabled = true;
            */
        }
        #endregion
    }
}
