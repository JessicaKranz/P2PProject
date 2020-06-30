using CommonLogic;
using Datenmodelle;
using System.Collections.Generic;

namespace PeerToPeerCloneC
{
    class Program
    {
        public static List<IP> serverAddresses = new List<IP>()
        {
            new IP("127.0.0.1", 13002),
            new IP("127.0.0.1", 13003)
        };

        static List<IP> tcpClientAdresses = new List<IP>()
        {
            new IP("127.0.0.1", 13000),
            new IP("127.0.0.1", 13001)
        };

        static void Main(string[] args)
        {
            TcpConnection tcpConnection = new TcpConnection();
            tcpConnection.StartServersAndClients(serverAddresses, tcpClientAdresses);
        }
    }
}