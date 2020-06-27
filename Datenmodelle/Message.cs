using System;
using System.ComponentModel;
using System.Linq;
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



        public string Type { get; private set; }
        /// <summary>
        /// EntryPeer and JoinPeer use TTLs differently
        /// </summary>
        public string Ttl { get; private set; }               
        /// <summary>
        /// target ID
        /// </summary>
        public string DestinationId { get; private set; }  
        /// <summary>
        /// senders ID
        /// </summary>
        public string SourceId { get; private set; }          
        /// <summary>
        /// senders name
        /// </summary>
        public string AuthorName { get; private set; }
        /// <summary>
        /// messages payload
        /// </summary>
        public string PlainText { get; private set; }
        

        
      
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

        public Message(string MessageClass, string TTL, string DestinationID, string OriginID, string authorName, string MessageText)
        {
            if (MessageClass.Length != FeldGroesseMessageClass)
            {
                throw new System.ArgumentException("MessageClass must be a string of length " +FeldGroesseMessageClass +".");
            }
            this.Type = MessageClass;

            if (TTL.Length != FeldGroesseTTL)
            {
                throw new System.ArgumentException("TTL must be a string of length " + FeldGroesseTTL + ".");
            }
            this.Ttl = TTL;

            if (DestinationID.Length != FeldGroesseDestinationID)
            {
                throw new System.ArgumentException("DestinationID must be a string of length " + FeldGroesseDestinationID + ".");
            }
            this.DestinationId = DestinationID;

            if (OriginID.Length != FeldGroesseOriginID)
            {
                throw new System.ArgumentException("OriginID must be a string of length " + FeldGroesseOriginID + ".");
            }
            this.SourceId = OriginID;
            AuthorNameLength = authorName.Length;
            StartIndexMessageText = AuthorNameLength + FeldGroesseNameLength;
            string AuthorNameLengthString = this.GetAuthorNameLength();
            if (AuthorNameLength > 0)
            {
                this.AuthorName = authorName;
            }
            else
            {
                this.AuthorName = OriginID;
                AuthorNameLength = 8;
                AuthorNameLengthString = this.GetAuthorNameLength();
            }
            this.PlainText = MessageText;           
        }     
    }
}
