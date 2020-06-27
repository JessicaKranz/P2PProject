using Datenmodelle;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;

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
        public static void ThreadProc1()
        {
            BeTheServer(13000);
        }

        public static void ThreadProc2()
        {
            BeTheServer(13003);
        }

        static bool go_on = true;
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

            Thread.CurrentThread.Name = "Main";
            Thread t = new Thread(new ThreadStart(ThreadProc1));

            t.Start();

            Thread t1 = new Thread(new ThreadStart(ThreadProc2));
            t1.Start();


            //t.Join();


            Task taskConnect1;
            Task taskConnect2;


            while (true)
            {
                taskConnect1 = new Task(() => Connect("127.0.0.1", 13001));
                taskConnect1.Start();

                taskConnect2 = new Task(() => Connect("127.0.0.1", 13002));
                taskConnect2.Start();

            }

            taskConnect1.Wait();
            taskConnect2.Wait();

        }


        static void Connect(String server, Int32 port)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.

                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data;

                while (go_on)
                {
                    string line = Console.ReadLine();
                    data = System.Text.Encoding.ASCII.GetBytes(line);

                    // Get a client stream for reading and writing.
                    //  Stream stream = client.GetStream();

                    NetworkStream stream = client.GetStream();

                    // Send the message to the connected TcpServer.
                    stream.Write(data, 0, data.Length);

                    //Console.WriteLine("Sent: {0}", line);

                    // Receive the TcpServer.response.

                    // Buffer to store the response bytes.
                    data = new Byte[256];

                    // String to store the response ASCII representation.
                    String responseData = String.Empty;

                    // Read the first batch of the TcpServer response bytes.
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);

                    if (line.Equals("quit"))
                    {
                        go_on = false;
                        // Close everything.
                        stream.Close();
                        client.Close();
                    }
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

            Console.WriteLine("\n Disconnected");
            Console.Read();
        }



        static void BeTheServer(Int32 port)
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

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        //Console.WriteLine("Sent: {0}", data);
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
