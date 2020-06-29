using CommonLogic;
using Datenmodelle;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PeerToPeerCloneA
{
    class Program
    {

        private static Random rand = new Random();
        private static Fish myFish = new Fish(rand.Next(), rand.NextDouble());
        private static Timer myFishTimer;
        private static int wunschAnzahlNachbarn;
        private static int peerID = rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1); //Erzeugt Zahlenzwischen 10.000.000 und 99.999.999
        private static string myName = peerID.ToString();
        private static List<Peer> bekanntePeers; //TODO Liste wo die Nachbarn abgespeichert sind. ABer wie ? Als Neighbours? neue Datenstrucktur? Nur IDs? auch IP's??
        private static List<Connection> neighbours;  //TODO Liste wo die Nachbarn abgespeichert sind. ABer wie ? Als Neighbours? neue Datenstrucktur? Nur IDs? auch IP's??

        static void Main(string[] args)
        {
            /*Nachrichtentest Testblock: */
            {

                string test = "tt00091122334422334411123ThisNameIsCrazyLongWhyAmIEvenConsideringSuchALongNameThisISGonnaFlyUmMy arsAtSomePointMaybeIshouldtdreiundzwahnzigzeichen [13:00:22] This is the PlaintextMessage. Why am I even sending this.";
                Message nachricht = new Message(test);
                Console.WriteLine("-------------Nachricht Test Block Anfang-------------");
                Console.WriteLine("Nachricht MessageClass: " + nachricht.Type);
                Console.WriteLine("Nachricht TTL         : " + nachricht.Ttl);
                Console.WriteLine("Nachricht Destination : " + nachricht.DestinationId);
                Console.WriteLine("Nachricht Origin      : " + nachricht.SourceId);
                Console.WriteLine("Laenge des Authornamen: " + nachricht.GetAuthorNameLength());
                Console.WriteLine("Author der Nachricht  : " + nachricht.AuthorName);
                Console.WriteLine("Nachricht Plaintext   : " + nachricht.PlainText);

                ProzessNachricht(nachricht);
                Console.WriteLine("-------------Nachricht Test Block Ende-------------");
                Console.WriteLine("");
                Console.WriteLine("Ab hier neue Nachricht");

                string test1 = "PE9998" + rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1) + rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1) + "000192.168.178.55";
                string test2 = "PE9994" + rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1) + rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1) + "008Thorsten10.8.0.8";
                nachricht = new Message(test1);
                Console.WriteLine("-------------Nachricht Test Block Anfang-------------");
                Console.WriteLine("Nachricht MessageClass: " + nachricht.Type);
                Console.WriteLine("Nachricht TTL         : " + nachricht.Ttl);
                Console.WriteLine("Nachricht Destination : " + nachricht.DestinationId);
                Console.WriteLine("Nachricht Origin      : " + nachricht.SourceId);
                Console.WriteLine("Laenge des Authornamen: " + nachricht.GetAuthorNameLength());
                Console.WriteLine("Author der Nachricht  : " + nachricht.AuthorName);
                Console.WriteLine("Nachricht Plaintext   : " + nachricht.PlainText);
                ProzessNachricht(nachricht);
                Console.WriteLine("-------------Nachricht Test Block Ende-------------");

                Console.WriteLine("");
                Console.WriteLine("Ab hier neue Nachricht");
                nachricht = new Message(test2);
                Console.WriteLine("-------------Nachricht Test Block Anfang-------------");
                Console.WriteLine("Nachricht MessageClass: " + nachricht.Type);
                Console.WriteLine("Nachricht TTL         : " + nachricht.Ttl);
                Console.WriteLine("Nachricht Destination : " + nachricht.DestinationId);
                Console.WriteLine("Nachricht Origin      : " + nachricht.SourceId);
                Console.WriteLine("Laenge des Authornamen: " + nachricht.GetAuthorNameLength());
                Console.WriteLine("Author der Nachricht  : " + nachricht.AuthorName);
                Console.WriteLine("Nachricht Plaintext   : " + nachricht.PlainText);
                Console.WriteLine("");
                ProzessNachricht(nachricht);
                Console.WriteLine("-------------Nachricht Test Block Ende-------------");


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

            switch (n.Type)
            {
                case PeerEntry:
                    try
                    {
                        PeerEntryMethod(n);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something went wrong. Defenetly something related to the Message processing. Propably someting about your TTL.");
                    }
                    break;
                case PeerJoin:
                    try
                    {
                        PeerJoinMethod(n);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something went wrong. Defenetly something related to the Message processing.");
                    }
                    break;
                case PersonalMessage:
                    //TODO dostuff()
                    break;
                case GroupMessage:
                    //TODO dostuff()
                    break;
                case FishTank:
                    //TODO dostuff()
                    break;
                case WannabeNeighbour:
                    //TODO dostuff()
                    break;
            }
        }
        static void PeerEntryMethod(Message n)
        {
            if (n.Ttl.Substring(0, 3) != "999")
            {
                throw new System.ArgumentException(" Have received Invalid Message with: Peer Entry request but with TTL != 999x ");
            }
            int wishedNeighbours = Int32.Parse(n.Ttl.Substring(3, 1)); //vierte stelle der TTL
            if (WillIBecomeANewNeighbour())
            {
                wishedNeighbours--;
                string ipAdresse = n.PlainText;
                ConnectTCP(ipAdresse);
            }
            if (wishedNeighbours > 0)
            {
                SendPJMessage(new Message
                {
                    Type = "PJ",
                    Ttl = "000" + wishedNeighbours,
                    DestinationId = n.DestinationId,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    PlainText = n.PlainText,
                });
            }
        }

        static void PeerJoinMethod(Message n)
        {
            if (n.Ttl.Substring(0, 3) != "000")
            {
                throw new System.ArgumentException(" Have received Invalid Message with: Peer Join propagation request but with TTL != 000x ");
            }
            int wishedNeighbours = Int32.Parse(n.Ttl.Substring(3, 1)); //vierte stelle der TTL gibt die Wunschzahl der Nachbarn an

            if (!neighbours.Contains(/*Wo Connection.parterPeer.GetPeerID() = n.OriginID*/new Connection("dummy")))//DO I KNOW THIS PEER ALREADY? Schaue in der Neighbours Liste nach 
            {
                Console.WriteLine("TODO HERE YOU MUST REJECT THE OFFER BUT STILL CARRY ON!");
                if (WillIBecomeANewNeighbour())
                {
                    wishedNeighbours--;
                    string ipAdresse = n.PlainText;
                    ConnectTCP(ipAdresse);
                }
            }

            if (wishedNeighbours > 0)
            {
                SendPJMessage(new Message
                {
                    Type = "PJ",
                    Ttl = "000" + wishedNeighbours,
                    DestinationId = n.DestinationId,
                    SourceId = n.SourceId,
                    AuthorName = n.AuthorName,
                    PlainText = n.PlainText,
                });
            }
        }

        private static void SendPJMessage(Message pjMessage)
        {
            //TODO Send PJ MEssage over Overlay
        }

        static Boolean WillIBecomeANewNeighbour()
        {
            return true;
            Random rand1 = new Random();
            return rand1.NextDouble() < myFish.GetPortion();
        }

        static void ConnectTCP(string ipAdresse)
        {
            Console.WriteLine("Ich verbinde mich jetzt mit \"" + ipAdresse + "\"! (In wirklichkeit tue ich das noch nicht. Das kommt aber noch.");
            //TODO stelle verbindung mit dem Peer an folgender Ip.Adresse her. Fordere dafür alle informationen an die du brauchst. 
        }
    }
}
