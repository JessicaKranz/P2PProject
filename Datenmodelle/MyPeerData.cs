using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Datenmodelle
{
    public class MyPeerData
    {
        public string MyName { get; set; }
        Random Random = new Random();

        public int myPeerID { get; } //Erzeugt Zahlenzwischen 10.000.000 und 99.999.999
     
        public IPAddress myIPAddress { get; set; }    //Meine IP Addresse

        public List<GroupChat> myGroupChats { get; set; }

        public IP requestAddress { get; set; }
        public List<IP> serverAddresses { get; set; }
        public Dictionary<int, IP> tcpClientAddresses { get; set; }
        /// <summary>
        /// Dies sind die Nachbarn
        /// </summary>
        public List<TcpClient> tcpClients { get; set; } = new List<TcpClient>(); //neighbours

        public List<Message> seenMessages { get; set; } = new List<Message>();
        public List<IP> knownStablePeers { get; set; } = new List<IP>  
        {
            new IP("127.0.0.1", 13000),
            new IP("127.0.0.1", 13100),
            new IP("127.0.0.1", 13200)
        };

        public MyPeerData()
        {
            myPeerID = Random.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1);

            if (MyName == string.Empty)
            {
                MyName = "" + myPeerID;
            }
            myIPAddress = GetLocalIPAddress();

            myGroupChats = new List<GroupChat>();
            serverAddresses = new List<IP>();
            tcpClientAddresses = new Dictionary<int, IP>();
            knownStablePeers = new List<IP>();
        }
        
        
        /// <summary>
        /// select a port in the peers port range randomly and check if it is free
        /// else, redo
        /// </summary>
        /// <returns>an unassigned IP</returns>
        public IP GetNextFreePort()
        {
            try
            {
                int nextPort;
                do
                {
                    nextPort = Random.Next(requestAddress.port, requestAddress.port + 99);
                } while (this.serverAddresses.Any(x => x.port == nextPort));

                return new IP("127.0.0.1", nextPort);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Your request address is missing. Check your config. It's not gonna work without the request address set. Failed with {0}", ex.Message);
                return new IP(String.Empty, 0);
            }


        }

        private IPAddress GetLocalIPAddress()
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

    }
}
