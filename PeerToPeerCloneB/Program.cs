using CommonLogic;
using Datenmodelle;
using System.Collections.Generic;

namespace PeerToPeerCloneB
{
    class Program
    {
        static void Main(string[] args)
        {
            PeerData peer = new PeerData
            {
                serverAddresses = new List<IP>()
                {
                    new IP("127.0.0.1", 13100)
                },
                tcpClientAddresses = new List<IP>()
                {
                    
                },             
                requestAddress = new IP("127.0.0.1", 13100)
            };

            TcpConnection tcpConnection = new TcpConnection();
            tcpConnection.StartServersAndClients(peer);
        }
    }
}
