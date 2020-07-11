using BusinessLogic;
using Datenmodelle;
using System;
using System.Collections.Generic;
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
                tcpClientAddresses = new List<KeyValuePair<int, IP>>()
                {

                },
                requestAddress = new IP("127.0.0.1", 13200),
                knownStablePeers = new List<IP>()
                {
                    new IP("127.0.0.1", 13100),
                    new IP("127.0.0.1", 13000),

                },
                MyName = "PeerC"
            };

            Console.WriteLine(self.myPeerID);

            TcpConnection tcpConnection = new TcpConnection();
            tcpConnection.StartServers(self);
            Thread.Sleep(1000);
            if (self.tcpClientAddresses.Count < 2)
            {

                bool joinedSuccessfull = false;
                while (self.tcpClientAddresses.Count < 2)
                {
                    joinedSuccessfull = tcpConnection.ManageJoin(self);
                    Thread.Sleep(15000);
                }
            }
        }
    }
}