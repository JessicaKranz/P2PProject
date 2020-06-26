using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Schema;

namespace Datenmodelle
{
    public class Nachricht
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


        //Alle folgenden Char Arrays sind zu lesen als ASCII Schlüssel, die durch den Jeweiligen Peer interpretiert werden. 
        //Jeder Peer hat das selbe verständnis der Schlüssel
        private string MessageClass;        /* Was für eine Nachricht wird da Geschickt?*/
        private string TTL;                 /* Welche TTL hat die Nachricht [Bestimmte MessageClasses brauchen keine TTL -> dann ist sie entweder ohne Konsequenzen, oder wird anders benutzes (z.B. EntryPeer EP und JoinPeer JP) ]*/
        private string DestinationID;       /* Gibt es eine Destination in Form eines Gruppenchats/einzelnen Peers, dann steht sie hier*/
        private string OriginID;            /* Derjenige Peer, der die Nachricht ursprünglich losgeschickt hat [IDEE: man könnt hier auch eine Liste/Stack mitschicken mit dem letzen Absender immer hinten drann/oben drauf */
        //string OriginPort     = new char[5];
        private string AuthorName;              /* Gibt den Author der Nachricht mit Namen zurück */
        private string MessageText;         /* Falls es eine Nachricht gibt, steht sie hier */
        private string OriginalMessage;     /* Aus Debug Gründen, und weils nicht weh tut. Hier die Ganze Nachricht im Plaintext*/

        /*Variablen Getter*/
        public string GetMessageClass()
        {
            return MessageClass;
        }
        public string GetTTL()
        {
            return TTL;
        }
        public string GetDestinationID()
        {
            return DestinationID;
        }
        public string GetOriginID()
        {
            return OriginID;
        }
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
        public int GetAuthorNameLengthInt()
        {
            return AuthorNameLength;
        }
        public string GetAuthorName()
        {
            return AuthorName;
        }
        public string GetMessageText()
        {
            return MessageText;
        }
        public string GetOriginalMessage()
        {
            return OriginalMessage;
        }
        /*Variablen Getter Ende*/


        public Nachricht(string message)
        {
            OriginalMessage = message;

            MessageClass    = message.Substring(StartIndexMessageClass , FeldGroesseMessageClass    );
            TTL             = message.Substring(StartIndexTTL          , FeldGroesseTTL             );
            DestinationID   = message.Substring(StartIndexDestinationID, FeldGroesseDestinationID   );
            OriginID        = message.Substring(StartIndexOriginID     , FeldGroesseOriginID        );
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
                AuthorName = OriginID;
                AuthorNameLength = 8;
            }
            StartIndexMessageText = StartIndexNameLength + FeldGroesseNameLength + AuthorNameLength;

            MessageText     = message.Substring(StartIndexMessageText);
        }

        public Nachricht(string MessageClass, string TTL, string DestinationID, string OriginID, string authorName, string MessageText)
        {
            if (MessageClass.Length != FeldGroesseMessageClass)
            {
                throw new System.ArgumentException("MessageClass must be a string of length " +FeldGroesseMessageClass +".");
            }
            this.MessageClass = MessageClass;

            if (TTL.Length != FeldGroesseTTL)
            {
                throw new System.ArgumentException("TTL must be a string of length " + FeldGroesseTTL + ".");
            }
            this.TTL = TTL;

            if (DestinationID.Length != FeldGroesseDestinationID)
            {
                throw new System.ArgumentException("DestinationID must be a string of length " + FeldGroesseDestinationID + ".");
            }
            this.DestinationID = DestinationID;

            if (OriginID.Length != FeldGroesseOriginID)
            {
                throw new System.ArgumentException("OriginID must be a string of length " + FeldGroesseOriginID + ".");
            }
            this.OriginID = OriginID;
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
            this.MessageText = MessageText;
            this.OriginalMessage = MessageClass + TTL + DestinationID + OriginID + AuthorNameLengthString+ AuthorName+ MessageText;
        }

        

    }
}
