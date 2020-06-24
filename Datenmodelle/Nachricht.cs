using System;
using System.Linq;
using System.Xml.Schema;

namespace Datenmodelle
{
    public class Nachricht
    {
        const int FeldGroesseMessageClass   =   2;
        const int FeldGroesseTTL            =   4;
        const int FeldGroesseDestinationID  =   16;
        const int FeldGroesseOriginID       =   16;

        const int StartIndexMessageClass    =   0;
        const int StartIndexTTL             =   StartIndexMessageClass    + FeldGroesseMessageClass;
        const int StartIndexDestinationID   =   StartIndexTTL             + FeldGroesseTTL;
        const int StartIndexOriginID        =   StartIndexDestinationID   + FeldGroesseDestinationID;
        const int StartIndexMessageText     =   StartIndexOriginID        + FeldGroesseOriginID;


        //Alle folgenden Char Arrays sind zu lesen als ASCII Schlüssel, die durch den Jeweiligen Peer interpretiert werden. 
        //Jeder Peer hat das selbe verständnis der Schlüssel
        private string MessageClass     = string.Empty;        /* Was für eine Nachricht wird da Geschickt?*/
        private string TTL              = string.Empty;                 /* Welche TTL hat die Nachricht [Bestimmte MessageClasses brauchen keine TTL -> dann ist sie ohne Konsequenzen ]*/
        private string DestinationID    = string.Empty;       /* Gibt es eine Destination in Form eines Gruppenchats/einzelnen Peers, dann steht sie hier*/
        private string OriginID         = string.Empty;            /* Derjenige Peer, der die Nachricht ursprünglich losgeschickt hat [IDEE: man könnt hier auch eine Liste/Stack mitschicken mit dem letzen Absender immer hinten drann/oben drauf */
        //string OriginPort     = new char[5];
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

            MessageClass    = message.Substring(StartIndexMessageClass , StartIndexMessageClass    + FeldGroesseMessageClass    );
            TTL             = message.Substring(StartIndexTTL          , StartIndexTTL             + FeldGroesseTTL             );
            DestinationID   = message.Substring(StartIndexDestinationID, StartIndexDestinationID   + FeldGroesseDestinationID   );
            OriginID        = message.Substring(StartIndexOriginID     , StartIndexOriginID        + FeldGroesseOriginID        );
            MessageText     = message.Substring(StartIndexMessageText);
        }
        

    }
}
