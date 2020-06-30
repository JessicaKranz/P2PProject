using CommonLogic;
using Datenmodelle;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace PeerToPeerCloneC
{
    class Program
    {
        public static List<IP> serverAddresses = new List<IP>()
        {
            new IP("127.0.0.1", 13002),
            //new IP("127.0.0.1", 13003)
        };

        static List<IP> tcpClientAdresses = new List<IP>()
        {
            //new IP("127.0.0.1", 13000),
            new IP("127.0.0.1", 13001)
        };

        static void Main(string[] args)
        {
            //JOIN

            var entryPeer = new TcpClient(tcpClientAdresses.FirstOrDefault().address, tcpClientAdresses.FirstOrDefault().port);
            //Start one threads that manages the connection to all communication partners
            new Thread(o => TcpConnection.Join(entryPeer)).Start();

            TcpConnection tcpConnection = new TcpConnection();

            tcpConnection.StartServersAndClients(serverAddresses, tcpClientAdresses);
        }
    }
}