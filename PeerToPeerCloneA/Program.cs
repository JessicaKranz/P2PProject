using CommonLogic;
using Datenmodelle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace PeerToPeerCloneA
{
    class Program
    {
        public static List<IP> serverAddresses = new List<IP>()
        {
            new IP("127.0.0.1", 13000)
        };

        static List<IP> tcpClientAdresses = new List<IP>()
        {
            new IP("127.0.0.1", 13003)
        };

        #region vardef
        private static Random rand = new Random();
        private static Fish myFish = new Fish(rand.Next(), rand.NextDouble());
        private static System.Timers.Timer myFishTimer;
        private static int wunschAnzahlNachbarn;
        private static int peerID = rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1); //Erzeugt Zahlenzwischen 10.000.000 und 99.999.999
        private static string myName = peerID.ToString();
        private static List<Peer> bekanntePeers;    //TODO Liste wo bekannte Peers als Peers  abgespeichert sind. GRUNDSATZFRAGE: brauchen wir das?
        private static List<Connection> neighbours;  //TODO Liste wo die Nachbarn abgespeichert sind. Als Connection. Mit jeweils eigenem und gegenteiligem Peer object (brauch man das?) , ipAddresse und PortNummber
        private static IPAddress myIPAddress = GetLocalIPAddress();     //Meine IP Addresse
        private static List<int> myGroupChatsIDs = new List<int>();                    //Die nächste drei Listen sind streng Abhänig voneinander
        private static List<List<Peer>> groupChatMembers = new List<List<Peer>>();      //Zusammen Dokumentieren sie in welchen Group Chats der Aktuelle Peer ist, und welche anderen Peers auch in diesen Chats sind.
        private static List<string> myGroupChatsNames = new List<string>();             // Ich freue mich über einen eleganteren Weg. Bzw soll ich dafür eine Datenstrucktur anlegen?
        #endregion

        static void Main(string[] args)
        {
            #region Nachrichtentest Testblock:

            /*
            Message nachricht = new Message
                {
                    Type = "TT",
                    Ttl = 50,
                    DestinationId = 11223344,
                    SourceId = 22334411,
                    AuthorName = "ThisNameIsCrazyLongWhyAmIEvenConsideringSuchALongNameThisISGonnaFlyUmMy",
                    ChatMessage = "Hallo ich bin eine Chat Nachricht",
                    //Fish = new Fish { 1000, 0.5 }, Das hier funktioniert nicht. ALternative ist soweit ich das sehe nur "hinterher"
                    SendersIP = IPAddress.Parse("192.168.123.11"), // Das Funktioniert                       
                };
            Console.WriteLine("-------------Nachricht Test Block Anfang-------------");
            Console.WriteLine("Nachricht MessageClass: " + nachricht.Type);
            Console.WriteLine("Nachricht TTL         : " + nachricht.Ttl);
            Console.WriteLine("Nachricht Destination : " + nachricht.DestinationId);
            Console.WriteLine("Nachricht Origin      : " + nachricht.SourceId);
            Console.WriteLine("Author der Nachricht  : " + nachricht.AuthorName);
            Console.WriteLine("Chat Nachricht        : " + nachricht.ChatMessage);
            Console.WriteLine("Entstehungzeitpunk    : " + nachricht.TimeStamp);
            ProzessNachricht(nachricht);
            Console.WriteLine("-------------Nachricht Test Block Ende-------------");
            Console.WriteLine("");
            Console.WriteLine("Ab hier neue Nachricht");

            nachricht = new Message
            {
                Type = "PE",
                Ttl = 100,
                DestinationId = rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1),
                SourceId = rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1),
                AuthorName = "Kevin",
                ChatMessage = "Ich bin voll so assi und so",
                //Fish = new Fish { 1000, 0.5 }, Das hier funktioniert nicht. ALternative ist soweit ich das sehe nur "hinterher"
                SendersIP = IPAddress.Parse("192.168.22.15"), // Das Funktioniert                       
            };
            Console.WriteLine("-------------Nachricht Test Block Anfang-------------");
            Console.WriteLine("Nachricht MessageClass: " + nachricht.Type);
            Console.WriteLine("Nachricht TTL         : " + nachricht.Ttl);
            Console.WriteLine("Nachricht Destination : " + nachricht.DestinationId);
            Console.WriteLine("Nachricht Origin      : " + nachricht.SourceId);
            Console.WriteLine("Author der Nachricht  : " + nachricht.AuthorName);
            Console.WriteLine("Nachricht Chat Message: " + nachricht.ChatMessage);
            Console.WriteLine("Senders IP            : " + nachricht.SendersIP);
            ProzessNachricht(nachricht);
            Console.WriteLine("-------------Nachricht Test Block Ende-------------");

            Console.WriteLine("");
            Console.WriteLine("Ab hier neue Nachricht");
            nachricht = new Message
            {
                Type = "TT",
                Ttl = 50,
                DestinationId = rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1),
                SourceId = rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1),
                AuthorName = "Thorsten Heinrich Theobald von und zu Wolfenstein",
                ChatMessage = "Für sie bin ich immer noch Dr. Wolfenstein! Herr Doktor Wolfenstein!",
                //Fish = new Fish { 1000, 0.5 }, Das hier funktioniert nicht. ALternative ist soweit ich das sehe nur "hinterher"
                SendersIP = IPAddress.Parse("88.88.88.88"), // Das Funktioniert                       
            };
            Console.WriteLine("-------------Nachricht Test Block Anfang-------------");
            Console.WriteLine("Nachricht MessageClass: " + nachricht.Type);
            Console.WriteLine("Nachricht TTL         : " + nachricht.Ttl);
            Console.WriteLine("Nachricht Destination : " + nachricht.DestinationId);
            Console.WriteLine("Nachricht Origin      : " + nachricht.SourceId);
            Console.WriteLine("Author der Nachricht  : " + nachricht.AuthorName);
            Console.WriteLine("Nachricht Plaintext   : " + nachricht.ChatMessage);
            Console.WriteLine("Senders IP            : " + nachricht.SendersIP);
            Console.WriteLine("");
            ProzessNachricht(nachricht);
            Console.WriteLine("-------------Nachricht Test Block Ende-------------");
            */
            #endregion

            TcpConnection tcpConnection = new TcpConnection();
            tcpConnection.StartServersAndClients(serverAddresses, tcpClientAdresses);         
        }
        #region var def NachrichtenLogik
        const string PeerEntry = "PE";
        const string PeerJoin = "PJ";
        const string PersonalMessage = "PM";
        const string GroupMessage = "GM";
        const string FishTank = "FT";
        const string WannabeNeighbour = "WN";

        //const string 


        #endregion
        #region EingehendeNachrichtenLogik 
        /*Hier passiert die Logic eines Peers. Hier steht was er bei welchem Nachrichtentyp Macht etc*/
        private static void ProzessNachricht(Message n)
        {
            //TODO entweder hier oder woanders: Check ob man diese Nachricht schonmal gesehn hat über n.Timestamp und n.origin, n.destination. Man braucht dafür ne Liste.
            if (null != bekanntePeers.Where(x => x.GetPeerID() == n.SourceId && x.GetAssociatedName() == n.AuthorName))
            {
                bekanntePeers.Add(new Peer(n.SourceId, n.AuthorName));
            }
            else if (null != bekanntePeers.Where(x => x.GetPeerID() == n.SourceId))
            {
                bekanntePeers.Find(x => x.GetPeerID() == n.SourceId).SetAssosiatedName(n.AuthorName);
            }
            else
            {
                bekanntePeers.Find(x => x.GetPeerID() == n.SourceId && x.GetAssociatedName() == n.AuthorName).UpdateLastSeen();
            }

            switch (n.Type)
            {
                case PeerEntry:
                    IncommingPeerEntryMessage(n);
                    break;
                case PeerJoin:
                    IncommingPeerJoinMessage(n);
                    break;
                case PersonalMessage:
                    IncommingPersonalMessage(n);
                    break;
                case GroupMessage:
                    IncommingGroupMessage(n);
                    break;
                case FishTank:
                    IncommingFishTankMessage(n);
                    break;
                case WannabeNeighbour:
                    //TODO dostuff()
                    break;
            }
        }
        static void IncommingPeerEntryMessage(Message n)
        {
            n.Ttl = (int)(n.WishedNeighbours * 3 * (1 + 1 / myFish.GetPortion()));          // Erzeuge eine TTL die Zukksessiv heruntergesetzt wird, damit es im Overlay keine Geisternachrichten gibt.
            if (WillIBecomeANewNeighbour())
            {
                n.WishedNeighbours--;
                ConnectTCP(n.SendersIP, new Peer(n.SourceId, n.AuthorName));
            }
            if (n.WishedNeighbours > 0 && n.Ttl > 0)
            {
                SendMessageFloodOverlay(new Message
                {
                    Type = "PJ",
                    Ttl = n.Ttl,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    SendersIP = n.SendersIP,

                });
            }
        }

        static void IncommingPeerJoinMessage(Message n)
        {
            n.Ttl--;

            if (null == neighbours.Where(x => x.partnerIPAddress == n.SendersIP))
            {
                if (WillIBecomeANewNeighbour())
                {
                    n.WishedNeighbours--;
                    ConnectTCP(n.SendersIP, new Peer(n.SourceId, n.AuthorName));
                }
            }

            if (n.WishedNeighbours > 0)
            {
                SendMessageFloodOverlay(new Message
                {
                    Type = "PJ",
                    Ttl = n.Ttl,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    SendersIP = n.SendersIP,

                });
            }
        }

        static void IncommingPersonalMessage(Message n)
        {
            n.Ttl--;
            if (n.DestinationId == peerID)
            {
                //Maybe Find a better way to deliver
                Console.WriteLine("[" + n.TimeStamp + n.AuthorName + "] " + " wrote to you:" + n.ChatMessage);
            }
            else if (null != neighbours.Where(x => x.partnerPeer.GetPeerID() == n.DestinationId))
            {
                SendMessage(n, neighbours.Where(x => x.partnerPeer.GetPeerID() == n.DestinationId).FirstOrDefault());
            }
            else
            {
                SendMessageFloodOverlay(n);
            }
        }

        static void IncommingGroupMessage(Message n)
        {
            n.Ttl--;
            if (myGroupChatsIDs.Contains(n.DestinationId))
            {
                //Maybe Find a better way to deliver
                Console.WriteLine("[" + n.TimeStamp + n.AuthorName + "] " + " wrote in \"" + n.GroupChatName + "\":" + n.ChatMessage);
            }
            else
            {
                SendMessageFloodOverlay(n);
            }
        }

        static void IncommingFishTankMessage(Message n)
        {
            if (n.Fish.GetSize() > myFish.GetSize())
            {
                myFish = n.Fish;
                Reset(myFishTimer);
            }
            else if (n.Fish.GetSize() > myFish.GetSize())
            {
                myFish.SetPortion(myFish.GetPortion() + n.Fish.GetPortion());
                Reset(myFishTimer);
            }

        }

        #endregion

        #region Ausgehende Nachrichten Logik
        public static void SendPeerEntryMessage()
        {
            //FOR THIS WE DONT HAVE A CONNECTION YET? WHAT TO DO?

            //SendMessage(new Message { },new Connection )
        }

        private static void SendFTMessage()
        {
            myFish.SetPortion(myFish.GetPortion() / neighbours.Count() + 1); //Aktualisiere den eigenen Fish und versende Ihn. //TOTALK muss man das Synchronized machen?
            SendMessageFloodOverlay(new Message
            {
                Fish = myFish,
                SourceId = peerID,
                AuthorName = myName,
            });
        }

        public static void SendPersonalMessage(string PM, int destinationID)
        {
            SendMessageFloodOverlay(new Message
            {
                Type = "PM",
                Ttl = 7, //(int)(3 * (1 + 1 / myFish.GetPortion())), //TODO CHECK IF PMS REACH DESTINATION. OTHERWISE MAKE TTL HIGHER Maybe the given Algorithm will work
                AuthorName = myName,
                DestinationId = destinationID,
                SourceId = peerID,
                ChatMessage = PM,
            });
        }
        public static void SendGroupMessage(string GM, int destinationID)
        {

        }

        /// <summary>
        /// Versendet die Nachricht n an einen zufälligen Nachbarn im Overlay 
        /// </summary>
        /// <param name="n"></param>
        private static void SendMessageSingularRandom(Message n)
        {
            //Maybe not send it back to previous Peer but not important
            var randomNeighbour = neighbours[rand.Next(neighbours.Count) - 1];
            SendMessage(n, randomNeighbour);
        }

        /// <summary>
        /// Versendet die Nachricht n an alle bekannten Nachbarn (Die Gesamte neigbours Liste)
        /// </summary>
        /// <param name="n"></param>
        private static void SendMessageFloodOverlay(Message n)
        {
            //Maybe not send it back to previous Peer but not important (n.SourceId tut nicht immer das gewünschte.
            foreach (Connection neighbour in neighbours)
            {
                SendMessage(n, neighbour);
            }
        }

        /// <summary>
        /// Sendet Nachricht n an den Peer mit dem er durch Connection c verbunden ist
        /// </summary>
        /// <param name="n"></param>
        /// <param name="c"></param>
        private static void SendMessage(Message n, Connection c)
        {
            //TODO 
        }

        #endregion
        #region Allerlei Funktionen
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        static void ConnectTCP(IPAddress ipAdresse, Peer newNeighbour)
        {

            Console.WriteLine("Ich verbinde mich jetzt mit \"" + ipAdresse + "\"! (In wirklichkeit tue ich das noch nicht. Das kommt aber noch.");
            int neigboursPortHeUsesToTalkToMe = 666; // TODO OR TODELETE FROM CONNECTION DATENMODELL
            int myPortIUseToTalkWithHim = 666;       // TODO OR TODELETE FROM CONNECTION DATENMODELL
            neighbours.Add(new Connection(newNeighbour, ipAdresse, neigboursPortHeUsesToTalkToMe, new Peer(peerID, myName), myIPAddress, myPortIUseToTalkWithHim));
            //TODO stelle verbindung mit dem Peer an folgender Ip.Adresse her. Fordere dafür alle informationen an die du brauchst. 
        }
        static Boolean WillIBecomeANewNeighbour()
        {
            return true; // TODO Should be like this below. Just delet stuff on the left.
            Random rand1 = new Random();
            return rand1.NextDouble() < myFish.GetPortion();
        }
        public static void Reset(System.Timers.Timer timer)
        {
            timer.Stop();
            timer.Start();
        }
        private static void SetFishTankTimer()
        {
            // Create a timer with a 60 second interval.
            myFishTimer = new System.Timers.Timer(60000);    // 1min
            // Hook up the Elapsed event for the timer. 
            myFishTimer.Elapsed += OnFishTimerEvent;
            myFishTimer.AutoReset = true;
            myFishTimer.Enabled = true;
        }
        private static void OnFishTimerEvent(Object source, ElapsedEventArgs e)
        {
            SendFTMessage();
        }
        #endregion
    }


}
