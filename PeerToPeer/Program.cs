using CommonLogic;
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
            PeerData peer = new PeerData
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
            if (peer.tcpClientAddresses.Count == 0)
            {
                var selectedStablePeer = peer.knownStablePeers.ElementAt(random.Next(peer.knownStablePeers.Count));
                new Thread(o => tcpConnection.Join(peer.GetNextFreePort(), selectedStablePeer)).Start();
            }
            tcpConnection.StartServersAndClients(peer);
        }
    }
}