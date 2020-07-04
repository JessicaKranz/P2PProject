using BusinessLogic;
using Datenmodelle;
using System;
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
            MyPeerData self = new MyPeerData
            {
                serverAddresses = new List<IP>()
                {
                    new IP("127.0.0.1", 13200),
                    //new IP("127.0.0.1", 13003)
                },
                tcpClientAddresses = new List<IP>()
                {

                },
                requestAddress = new IP("127.0.0.1", 13200),
                knownStablePeers = new List<IP>()
                {
                    new IP("127.0.0.1", 13100)
                },
            };

            Random random = new Random();

            TcpConnection tcpConnection = new TcpConnection();
            //JOIN
            //peer is not inside the network
            if (self.tcpClientAddresses.Count == 0)
            {
                var selectedStablePeer = self.knownStablePeers.ElementAt(random.Next(self.knownStablePeers.Count));
                new Thread(o => tcpConnection.Join(self, self.GetNextFreePort(), selectedStablePeer)).Start();
            }
            tcpConnection.StartServersAndClients(self);
        }
    }
}