using Datenmodelle;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests
{
    public class PeerTests
    {
        MyPeerData peer1;
        MyPeerData peer2;
        MyPeerData peer3;
        MyPeerData peer4;

        [SetUp]
        public void Setup()
        {
            peer1 = new MyPeerData
            {
                serverAddresses = new List<IP>()
            {
                new IP("127.0.0.1", 13000)
            },
                requestAddress = new IP("127.0.0.1", 13000)
            };

            peer2 = new MyPeerData
            {
                serverAddresses = new List<IP>()
            {
                new IP("127.0.0.1", 13100),
                new IP("127.0.0.1", 13105),
                new IP("127.0.0.1", 13106),
                new IP("127.0.0.1", 13107),
                new IP("127.0.0.1", 13111),
                new IP("127.0.0.1", 13112),
                new IP("127.0.0.1", 13113),
            },
                requestAddress = new IP("127.0.0.1", 13100)
            };

            peer3 = new MyPeerData
            {
                serverAddresses = new List<IP>()
            {
                new IP("127.0.0.1", 13200),
                new IP("127.0.0.1", 13299),
                new IP("127.0.0.1", 13201),
                new IP("127.0.0.1", 13202),
                new IP("127.0.0.1", 13203),
            },
                requestAddress = new IP("127.0.0.1", 13200)
            };

            peer4 = new MyPeerData
            {
                serverAddresses = new List<IP>() { },
            };
        }

        [Test]
        public void TestGetNextFreePort()
        {
            Assert.IsTrue(peer1.GetNextFreePort().port > 13000);
            Assert.IsTrue(peer1.GetNextFreePort().port < 13100);
            Assert.AreEqual(peer1.GetNextFreePort().address, "127.0.0.1");

            var p = peer2.GetNextFreePort().port;
            Assert.IsTrue
                (p > 13100 && p < 13105 ||
                 p > 13107 && p < 13111 ||
                 p > 13113 && p < 13200);
            Assert.AreEqual(peer2.GetNextFreePort().address, "127.0.0.1");

            p = peer3.GetNextFreePort().port;
            Assert.IsTrue(p > 13203 && p < 13299);

            Assert.AreEqual(peer4.GetNextFreePort().port, 0);
            Assert.AreEqual(peer4.GetNextFreePort().address, String.Empty);
        }
    }
}