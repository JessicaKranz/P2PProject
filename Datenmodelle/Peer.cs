using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Datenmodelle
{
    public class PeerData
    {
        public IP requestAddress { get; set; }
        public List<IP> serverAddresses { get; set; }
        public List<IP> tcpClientAddresses { get; set; }
        public List<IP> knownStablePeers { get; set; } = new List<IP>
        {
            new IP("127.0.0.1", 13000),
            new IP("127.0.0.1", 13100),
            new IP("127.0.0.1", 13200)
        };
        Random random = new Random();
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
                Console.WriteLine("You're request address is missing. Check your config. It's not gonna work without the request address set. Failed with {0}", ex.Message);
                return new IP(String.Empty, 0);
            }



        }
    }

    public class Peer
    {
        int peerID;
        string associatedName;
        DateTime lastSeen;

        public Peer(int peerID, string associatedName)
        {
            if (peerID < (int)Math.Pow(10, 7) || peerID > (int)Math.Pow(10, 8) - 1)
            {
                throw new System.ArgumentOutOfRangeException("peerID was out of Range. Please check creation of peerID.");
            }

            this.peerID = peerID;
            this.associatedName = associatedName;
            UpdateLastSeen();
        }

        public Peer(int peerID)
        {
            if (peerID < (int)Math.Pow(10, 7) || peerID > (int)Math.Pow(10, 8) - 1)
            {
                throw new System.ArgumentOutOfRangeException("peerID was out of Range. Please check creation of peerID.");
            }

            this.peerID = peerID;
            this.associatedName = "" + peerID;
            UpdateLastSeen();
        }
        public int GetPeerID()
        {
            return peerID;
        }
        public string GetAssociatedName()
        {
            return associatedName;
        }
        public void SetAssosiatedName(string associatedName)
        {
            this.associatedName = associatedName;
        }
        public DateTime GetLastSeen()
        {
            return lastSeen;
        }
        public void UpdateLastSeen()
        {
            lastSeen = DateTime.Now;
        }
    }
}
