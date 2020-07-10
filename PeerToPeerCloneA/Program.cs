using BusinessLogic;
using Datenmodelle;
using System.Collections.Generic;
using System.Threading;

namespace PeerToPeerCloneA
{
    class Program
    {
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

            MyPeerData self = new MyPeerData
            {
                serverAddresses = new List<IP>()
                {
                    new IP("127.0.0.1", 13300),
                    //new IP("127.0.0.1", 13003)
                },
                tcpClientAddresses = new List<IP>()
                {

                },
                requestAddress = new IP("127.0.0.1", 13300),
                knownStablePeers = new List<IP>()
                {
                    new IP("127.0.0.1", 13100),
                    new IP("127.0.0.1", 13200),
                },
            };


            TcpConnection tcpConnection = new TcpConnection();         
            tcpConnection.StartServers(self);
            Thread.Sleep(1000);
            if (self.tcpClientAddresses.Count < 2)
            {

                bool joinedSuccessfull = false;
                while (self.tcpClientAddresses.Count < 2)
                {
                    joinedSuccessfull = tcpConnection.ManageJoin(self);
                    Thread.Sleep(15000);
                }
            }
        }
    }
}