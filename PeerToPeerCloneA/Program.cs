using Datenmodelle;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PeerToPeerCloneA
{
    class Program
    {
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
                /*
                    string test = "tt000911112222333344442222333344441111 [13:00:22] This is the PlaintextMessage. Why am I even sending this.";
                    Nachricht nachricht = new Nachricht(test);
                    Console.WriteLine("-------------Nachricht Test Block Anfang-------------");
                    Console.WriteLine("Ursprungsnachricht: " + test);
                    Console.WriteLine("Nachricht MessageClass: " + nachricht.GetMessageClass());
                    Console.WriteLine("Nachricht TTL         : " + nachricht.GetTTL());
                    Console.WriteLine("Nachricht Destination : " + nachricht.GetDestinationID());
                    Console.WriteLine("Nachricht Origin      : " + nachricht.GetOriginID());
                    Console.WriteLine("Nachricht Plaintext   : " + nachricht.GetMessageText());
                    Console.WriteLine("Ursprungsnachricht: " + nachricht.GetOriginalMessage());
                    Console.WriteLine("-------------Nachricht Test Block Ende-------------");
                */
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

        
        const string peerEntry = "PE";
        const string peerJoin  = "PJ";

        /*Hier passiert die Logic eines Peers. Hier steht was er bei welchem Nachrichtentyp Macht etc*/
        static void ProzessNachricht(Nachricht n)
        {
            switch (n.GetMessageClass()) //This seems dirty aswell
            {
                case peerEntry:
                    //TODO dostuff()
                    break;
                case peerJoin:
                    //TODO dostuff()
                    break;

            }

                
        }
    }
}
