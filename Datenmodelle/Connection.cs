using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Datenmodelle
{
    public class Connection
    {
        Peer partnerPeer;
        string partnerIPAddress;
        string partnerPortNr;

        Peer mySelf;
        string myIPAddress;
        string myPortNr;

        //Maybe have Servers Running? Inhere in connections? Maybe Have Server datenstrucktur, die solche funktionalitäten übernimmt?

        public Connection(Peer partnerPeer, string partnerIPAddress, string partnerPortNr, Peer mySelf, string myIPAddress, string myPortNr)
        {
            this.partnerPeer = partnerPeer;
            this.partnerIPAddress = partnerIPAddress;
            this.partnerPortNr = partnerPortNr;

            this.mySelf = mySelf;
            this.myIPAddress = myIPAddress;
            this.myPortNr = myPortNr;
        }

        /// <summary>
        /// This was created Only for Test Cases and should soon seice existance. 
        /// </summary>
        /// <param name="dummy"></param>
        public Connection(string dummy)
        {
            if (dummy != "dummy")
            {
                throw new System.ArgumentException("You should not execute this code. This is just for testing purposes. If you want to test. Use \"dummy\" as the string input");
            }
            Random rand = new Random();
            this.partnerPeer = new Peer(""+ rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1));
            this.partnerIPAddress = "dummy";
            this.partnerPortNr = "dummy";

            this.mySelf = new Peer("" + rand.Next(1 * (int)Math.Pow(10, 7), 1 * (int)Math.Pow(10, 8) - 1));
            this.myIPAddress = "dummy";
            this.myPortNr = "dummy";

        }
    }
}
