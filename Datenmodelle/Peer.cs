using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;



namespace Datenmodelle
{
        public class Peer
    {
        public int peerID { get; }
        public string associatedName { get; set; }
        public DateTime lastSeen { get; set; }

        public Peer(int peerID, string associatedName)
        {
            if (peerID < (int)Math.Pow(10, 7) || peerID > (int)Math.Pow(10, 8) - 1)
            {
                throw new System.ArgumentOutOfRangeException("myPeerID was out of Range. Please check creation of myPeerID.");
            }

            this.peerID = peerID;
            this.associatedName = associatedName;
            UpdateLastSeen();
        }

        public Peer(int peerID)
        {
            if (peerID < (int)Math.Pow(10, 7) || peerID > (int)Math.Pow(10, 8) - 1)
            {
                throw new System.ArgumentOutOfRangeException("myPeerID was out of Range. Please check creation of myPeerID.");
            }

            this.peerID = peerID;
            this.associatedName = "" + peerID;
            UpdateLastSeen();
        }

        public void UpdateLastSeen()
        {
            lastSeen = DateTime.Now;
        }
    }
}
