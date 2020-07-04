using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Datenmodelle
{
    public class Connection
    {
        public Peer partnerPeer { get; set; }
        public IPAddress partnerIPAddress{get;set;}
        public int partnerPortNr {get;set;}

        public int myPortNr {get;set;} // Maybe Not neccesary TODO CHECK


    }
}
