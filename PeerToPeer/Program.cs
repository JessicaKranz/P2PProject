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
        static void Main(string[] args)
        {
            PeerData peer = new PeerData
            {
                serverAddresses = new List<IP>()
                {
                    new IP("127.0.0.1", 13002),
                    //new IP("127.0.0.1", 13003)
                },
                tcpClientAddresses = new List<IP>()
                {
                    //new IP("127.0.0.1", 13000),

                    //known online peer
                    new IP("127.0.0.1", 13001)
                },
            };
            //peer.tcpClient = new TcpClient(peer.tcpClientAddresses.FirstOrDefault().address, peer.tcpClientAddresses.FirstOrDefault().port);


            //JOIN

            TcpConnection tcpConnection = new TcpConnection();
          
            
            new Thread(o => tcpConnection.Join(peer.serverAddresses[0], peer.tcpClientAddresses.FirstOrDefault())).Start();

            tcpConnection.StartServersAndClients(peer);
        }
    }
}