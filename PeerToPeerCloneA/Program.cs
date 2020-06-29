using CommonLogic;
using Datenmodelle;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;



namespace PeerToPeerCloneA
{
    class Program
    {

        private static Random rand = new Random();
        private static Fish myFish = new Fish(rand.Next(), rand.NextDouble());
        private static System.Timers.Timer myFishTimer;
        private static int wunschAnzahlNachbarn;
        private static int peerID = rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1); //Erzeugt Zahlenzwischen 10.000.000 und 99.999.999
        private static string myName = peerID.ToString();
        private static List<Peer> bekanntePeers;    //TODO Liste wo bekannte peirs als Peers  abgespeichert sind. GRUNDSATZFRAGE: brauchen wir das?
        private static List<Connection> neighbours;  //TODO Liste wo die Nachbarn abgespeichert sind. Als Connection. Mit jeweils eigenem und gegenteiligem Peer object (brauch man das?) , ipAddresse und PortNummber


        static void Main(string[] args)
        {
            /*Nachrichtentest Testblock: */
            {
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
            }



            Thread server = new Thread(o => TcpConnection.Server(13000));
            server.Start();

            //Servers must be running before clients may connect
            Thread.Sleep(1000);

            List<TcpClient> tcpClients = new List<TcpClient>
            {
                new TcpClient("127.0.0.1", 13003)
            };
            Thread client = new Thread(o => TcpConnection.Client(tcpClients));
            client.Start();
        }



        const string PeerEntry = "PE"; //Eine Erfolgreiche PE Nachricht hat eine TTL "999x" wobei x auch ein Integer zwischen 1 und 9 ist. x gibt dabei die Zahl der Wunschnachbarn an.
                                       // Zusätzlich gilt bei einer PE Message, dass der Message teil leer ist, bis auf die IP.Addresse desjenigen Peers, der die Anfrage gestellt hat. 
        const string PeerJoin = "PJ";
        const string PersonalMessage = "PM";
        const string GroupMessage = "GM";
        const string FishTank = "FT";
        const string WannabeNeighbour = "WN";
        //const string 

        /*Hier passiert die Logic eines Peers. Hier steht was er bei welchem Nachrichtentyp Macht etc*/
        private static void ProzessNachricht(Message n)
        {
            if(!bekanntePeers.Contains(new Peer(n.SourceId, n.AuthorName))){ // TODO Ich will diese Abfrage ohne n.Author Name machen können (nur nach ID suchen)
                bekanntePeers.Add(new Peer(n.SourceId, n.AuthorName));
            }
            else if(bekanntePeers.Contains(new Peer(n.SourceId, n.AuthorName)))
            {
                bekanntePeers.Remove(new Peer(n.SourceId, n.AuthorName));    // TODO OLD AUTHORS NAME MUST BE REMOVED
                bekanntePeers.Add(new Peer(n.SourceId, n.AuthorName));      //  TODO Neuer AUTHORS Name kann eingefügt werden. Alternativ auch ein Update möglich
            }
            else
            {
                // bekanntePeers.Find(new Peer(n.SourceId, n.AuthorName)).UpdateLastSeen      //TODO Update Last Seen how?
            }
            
            switch (n.Type)
            {
                case PeerEntry:
                    PeerEntryMethod(n);
                    break;
                case PeerJoin:
                    PeerJoinMethod(n);
                    break;
                case PersonalMessage:
                    //TODO dostuff()
                    break;
                case GroupMessage:
                    //TODO dostuff()
                    break;
                case FishTank:
                    FishTankMethod(n);
                    break;
                case WannabeNeighbour:
                    //TODO dostuff()
                    break;
            }
        }
        static void PeerEntryMethod(Message n)
        {
            n.Ttl=(int)(n.WishedNeighbours*3*(1+1/myFish.GetPortion()));          // Erzeuge eine TTL die Zukksessiv heruntergesetzt wird, damit es im Overlay keine Geisternachrichten gibt.
            if (WillIBecomeANewNeighbour())
            {
                n.WishedNeighbours--;
                ConnectTCP(n.SendersIP, new Peer(n.SourceId,n.AuthorName));                                            
            }
            if (n.WishedNeighbours > 0 && n.Ttl > 0)
            {
                SendPJMessage(new Message
                {
                    Type = "PJ",
                    Ttl = n.Ttl,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    SendersIP = n.SendersIP,

                }) ;
            }
        }

        static void PeerJoinMethod(Message n)
        {
            n.Ttl--;

            if (!neighbours.Contains(/*Wo Connection.parterPeer.GetPeerID() = n.OriginID*/new Connection("dummy")))//DO I KNOW THIS PEER ALREADY? Schaue in der Neighbours Liste nach 
            {
                if (WillIBecomeANewNeighbour())
                {
                    n.WishedNeighbours--;
                    ConnectTCP(n.SendersIP, new Peer(n.SourceId, n.AuthorName));
                }
            }

            if (n.WishedNeighbours > 0)
            {
                SendPJMessage(new Message
                {
                    Type = "PJ",
                    Ttl = n.Ttl,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    SendersIP = n.SendersIP,

                });
            }
        }


        static void FishTankMethod(Message n)
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
        private static void SetFishTankTimer()
        {
            // Create a timer with a 60 second interval.
            myFishTimer = new System.Timers.Timer(60000);    // 1min
            // Hook up the Elapsed event for the timer. 
            myFishTimer.Elapsed += OnFishTimerEvent;
            myFishTimer.AutoReset = true;
            myFishTimer.Enabled = true;
        }
        private static void OnFishTimerEvent(Object source , ElapsedEventArgs e)
        {
            SendFTMessage();
        }


        private static void SendPJMessage(Message pjMessage)
        {
            //TODO Send PJ MEssage over Overlay
            //Pick a random Connection from: neighbours And send this Message to that Peer
        }
        private static void SendFTMessage()
        {
            myFish.SetPortion(myFish.GetPortion() / neighbours.Count()+1); //Aktualisiere den eigenen Fish und versende Ihn. //TOTALK muss man das Synchronized machen?
            Message FTMessage = new Message
                {
                    Fish = myFish,
                    SourceId = peerID,
                    AuthorName = myName,
                };
            //TODO Floode diese Message an Alle Nachbarn
        }

        static Boolean WillIBecomeANewNeighbour()
        {
            return true; // TODO Should be like this below. Just delet stuff on the left.
            Random rand1 = new Random();
            return rand1.NextDouble() < myFish.GetPortion();
        }

        static void ConnectTCP(IPAddress ipAdresse, Peer newNeighbour)
        {
            // neighbours

            Console.WriteLine("Ich verbinde mich jetzt mit \"" + ipAdresse + "\"! (In wirklichkeit tue ich das noch nicht. Das kommt aber noch.");
            //TODO stelle verbindung mit dem Peer an folgender Ip.Adresse her. Fordere dafür alle informationen an die du brauchst. 
        }

        /// <summary>
        /// Not Sure if this is correct but if it is, it should Reset a timer by stopping it and then reengaging it.
        /// </summary>
        /// <param name="timer"></param>
        public static void Reset(System.Timers.Timer timer)
        {
            timer.Stop();
            timer.Start();
        }
    }
}
