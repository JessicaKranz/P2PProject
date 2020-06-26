using System;
using System.Collections.Generic;
using System.Text;

namespace Datenmodelle
{
    public class Peer
    {
        string peerID; 
        string associatedName;
        DateTime lastSeen;

        //string peerIPAddress;       //Mit Fragezeichen hier stehen lassen -> doch weg gemacht
        //string connectionPortNr;    //Mit Fragezeichen hier stehen lassen -> doch weg gemacht

        public Peer(string peerID, string associatedName)
        {
            if(int.Parse(peerID)< Math.Pow(10, 7)||int.Parse(peerID)> (int)Math.Pow(10, 8) - 1)
            {
                throw new System.ArgumentOutOfRangeException("peerID was out of Range. Please check creation of peerID.");
            }

            this.peerID = peerID;
            this.associatedName = associatedName;
            UpdateLastSeen();     
        }

        public Peer(string peerID)
        {
            if (int.Parse(peerID) < Math.Pow(10, 7) || int.Parse(peerID) > (int)Math.Pow(10, 8) - 1)
            {
                throw new System.ArgumentOutOfRangeException("peerID was out of Range. Please check creation of peerID.");
            }

            this.peerID = peerID;
            this.associatedName = peerID;
            UpdateLastSeen();
        }
        public string GetPeerID()
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
