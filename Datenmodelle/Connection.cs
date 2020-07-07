using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Datenmodelle
{
    public class Connection
    {
        public Peer partnerPeer { get; set; }
        public IP partnerIP{get;set;}
        public IP myIP {get;set;}
    }
}
