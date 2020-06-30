using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Datenmodelle
{
    public class Connection
    {
        public Peer partnerPeer;
        public IPAddress partnerIPAddress;
        int partnerPortNr;

        Peer mySelf;
        IPAddress myIPAddress;
        int myPortNr;

        //Maybe have Servers Running? Inhere in connections? Maybe Have Server datenstrucktur, die solche funktionalitäten übernimmt?

        public Connection(Peer partnerPeer, IPAddress partnerIPAddress, int partnerPortNr, Peer mySelf, IPAddress myIPAddress, int myPortNr)
        {
            this.partnerPeer = partnerPeer;
            this.partnerIPAddress = partnerIPAddress;
            this.partnerPortNr = partnerPortNr;

            this.mySelf = mySelf;
            this.myIPAddress = myIPAddress;
            this.myPortNr = myPortNr;
        }
    }
}
