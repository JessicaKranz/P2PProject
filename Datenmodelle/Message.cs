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



        public string Type { get; set; }
        /// <summary>
        /// EntryPeer and JoinPeer use TTLs differently
        /// </summary>
        public string Ttl { get; set; }               
        /// <summary>
        /// target ID
        /// </summary>
        public string DestinationId { get; set; }  
        /// <summary>
        /// senders ID
        /// </summary>
        public string SourceId { get; set; }          
        /// <summary>
        /// senders name
        /// </summary>
        public string AuthorName { get; set; }
        /// <summary>
        /// messages payload
        /// </summary>
        public string PlainText { get; set; }
        

        
      
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
