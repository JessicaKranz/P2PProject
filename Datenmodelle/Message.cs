using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Xml.Schema;

namespace Datenmodelle
{
    public class Message
    {
        const int FeldGroesseMessageClass   =   2;
        const int FeldGroesseTTL            =   4;
        const int FeldGroesseDestinationID  =   8;
        const int FeldGroesseOriginID       =   8;
        const int FeldGroesseNameLength     =   3;

        const int StartIndexMessageClass    =   0;
        const int StartIndexTTL             =   StartIndexMessageClass    + FeldGroesseMessageClass;
        const int StartIndexDestinationID   =   StartIndexTTL             + FeldGroesseTTL;
        const int StartIndexOriginID        =   StartIndexDestinationID   + FeldGroesseDestinationID;
        const int StartIndexNameLength      =   StartIndexOriginID        + FeldGroesseOriginID;
        
        private int AuthorNameLength;
        private int StartIndexMessageText ;



        public string Type { get; set; }
        /// <summary>
        /// EntryPeer and JoinPeer use TTLs differently
        /// </summary>
        public string Ttl { get; set; }             //TODO CHANGE TO INT ONCE SERIALISING AND DESERIALISING IS A THING!                 
        /// <summary>
        /// target ID
        /// </summary>
        public string DestinationId { get; set; }  //TODO CHANGE TO INT ONCE SERIALISING AND DESERIALISING IS A THING!  (Maybe Check for valid input)
        /// <summary>
        /// senders ID
        /// </summary>
        public string SourceId { get; set; }          //TODO CHANGE TO INT ONCE SERIALISING AND DESERIALISING IS A THING!  (Maybe Check for valid input)
        /// <summary>
        /// senders name
        /// </summary>
        public string AuthorName { get; set; } 
        /// <summary>
        /// messages payload
        /// </summary>
        public string PlainText { get; set; }

        /// <summary>
        /// TimeStamp of when the Message was Sent
        /// </summary>
        public DateTime TimeStamp { get; set; }  //TODO TALK TO JESSI ABOUT MAYBE CREATING THIS IN THE CONSTRUKTOR!

        /// <summary>
        /// A fish that is passed around in the network, to approximate the number of Peers connected.
        /// </summary>
        public Fish Fish { get; set; } //TODO CHANGE TO INT ONCE SERIALISING AND DESERIALISING IS A THING!

        /// <summary>
        /// IP Address from the Sender of the Original Message
        /// </summary>
        public IPAddress SendersIP { get; set; } 

        /// <summary>
        /// The Number of neighbours a joining Peer to the Overlay wants to have.
        /// </summary>
        public int WishedNeighbours { get; set; }
        

        
      
        public string GetAuthorNameLength()
        {
            if (AuthorNameLength > (int)Math.Pow(10, FeldGroesseNameLength) - 1)
            {
                throw new System.ArgumentException("Author Name To long");
            }
            else if (AuthorNameLength > (int)Math.Pow(10, FeldGroesseNameLength - 1) - 1)
            {
                return AuthorNameLength.ToString();
            }
            else if (AuthorNameLength > (int)Math.Pow(10, FeldGroesseNameLength - 2) - 1)
            {
                return "0" + AuthorNameLength.ToString();
            }
            else if (AuthorNameLength > (int)Math.Pow(10, FeldGroesseNameLength - 3) - 1)
            {
                return "00" + AuthorNameLength.ToString();
            }
            else
            {
                return "000";
            }
        }
    
        public Message(){}


        public Message(string message)
        {         
            Type            = message.Substring(StartIndexMessageClass , FeldGroesseMessageClass    );
            Ttl             = message.Substring(StartIndexTTL          , FeldGroesseTTL             );
            DestinationId   = message.Substring(StartIndexDestinationID, FeldGroesseDestinationID   );
            SourceId        = message.Substring(StartIndexOriginID     , FeldGroesseOriginID        );
            for (int i = 0; i < FeldGroesseNameLength; i++)
            {
                AuthorNameLength = AuthorNameLength + int.Parse(message.Substring(StartIndexNameLength + i, 1))*(int)Math.Pow(10,FeldGroesseNameLength-1-i);
                Console.WriteLine(AuthorNameLength);
            }

            if (AuthorNameLength > 0 ) {
                AuthorName = message.Substring(StartIndexNameLength + FeldGroesseNameLength, AuthorNameLength);
            }
            else
            {
                AuthorName = SourceId;
                AuthorNameLength = 8;
            }
            StartIndexMessageText = StartIndexNameLength + FeldGroesseNameLength + AuthorNameLength;

            PlainText     = message.Substring(StartIndexMessageText);
        }
    }
}
