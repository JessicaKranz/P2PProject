using System;
using System.Collections.Generic;
using System.Text;

namespace Datenmodelle
{
    public class Peer
    {
        int peerID; 
        string associatedName;
        DateTime lastSeen;

        public Peer(int peerID, string associatedName)
        {
            if(peerID < (int) Math.Pow(10, 7)|| peerID > (int)Math.Pow(10, 8) - 1)
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
