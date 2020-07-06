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
        public string myName { get; set; }
        Random random = new Random();
        public Fish myFish { get; set; }
        public int wunschAnzahlNachbarn { get; set; }
        public int myPeerID { get; } //Erzeugt Zahlenzwischen 10.000.000 und 99.999.999
        public List<Peer> bekanntePeers { get; set; }    //TODO Liste wo BEKANNTE Peers als Peers eingetragen werden
        public List<Connection> neighbours { get; set; }  //TODO Liste wo die Nachbarn also Nachbar Peers abgespeichert sind. Als Connection. Mit jeweils eigenem und gegenteiligem Peer object (brauch man das?) , ipAddresse und PortNummber
        public IPAddress myIPAddress { get; set; }    //Meine IP Addresse

        public List<GroupChat> myGroupChats { get; set; }

        public IP requestAddress { get; set; }
        public List<IP> serverAddresses { get; set; }
        public List<IP> tcpClientAddresses { get; set; }
        public List<TcpClient> tcpClients { get; set; } = new List<TcpClient>();
        public List<IP> knownStablePeers { get; set; } = new List<IP>  //want to refactor into neigbours
        {
            new IP("127.0.0.1", 13000),
            new IP("127.0.0.1", 13100),
            new IP("127.0.0.1", 13200)
        };

        public MyPeerData()
        {
            myPeerID = random.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1);
            myFish = new Fish(random.Next(), random.NextDouble());

            if (myName == string.Empty)
            {
                myName = "" + myPeerID;
            }
            myIPAddress = GetLocalIPAddress();

            bekanntePeers = new List<Peer>();
            neighbours = new List<Connection>();
            myGroupChats = new List<GroupChat>();
            serverAddresses = new List<IP>();
            tcpClientAddresses = new List<IP>();
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
                    nextPort = random.Next(requestAddress.port, requestAddress.port + 99);
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
