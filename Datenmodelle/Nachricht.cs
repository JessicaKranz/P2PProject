using System;
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
        private char[] MessageClass = new char[FeldGroesseMessageClass];        /* Was für eine Nachricht wird da Geschickt?*/
        private char[] TTL = new char[FeldGroesseTTL];                          /* Welche TTL hat die Nachricht [Bestimmte MessageClasses brauchen keine TTL -> dann ist sie ohne Konsequenzen ]*/
        private char[] DestinationID = new char[FeldGroesseDestinationID];      /* Gibt es eine Destination in Form eines Gruppenchats/einzelnen Peers, dann steht sie hier*/
        private char[] OriginID = new char[FeldGroesseOriginID];                /* Derjenige Peer, der die Nachricht ursprünglich losgeschickt hat [IDEE: man könnt hier auch eine Liste/Stack mitschicken mit dem letzen Absender immer hinten drann/oben drauf */
        //char[] OriginPort     = new char[5];
        private string MessageText;                                             /* Falls es eine Nachricht gibt, steht sie hier */
        private string OriginalMessage;                                         /* Aus Debug Gründen, und weils nicht weh tut. Hier die Ganze Nachricht im Plaintext*/

        /*Variablen Getter*/
        public char[] GetMessageClass()
        {
            return MessageClass;
        }
        public char[] GetTTL()
        {
            return TTL;
        }
        public char[] GetDestinationID()
        {
            return DestinationID;
        }
        public char[] GetOriginID()
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

            for (int i = 0; i < FeldGroesseMessageClass; i++)
            {
                MessageClass[i] = message[i + StartIndexMessageClass];
            }

            for (int i = 0; i < FeldGroesseTTL; i++)
            {
                TTL[i] = message[i + StartIndexTTL];
            }

            for (int i = 0; i < FeldGroesseDestinationID; i++)
            {
                DestinationID[i] = message[i + StartIndexDestinationID];
            }

            for (int i = 0; i < FeldGroesseOriginID; i++)
            {
                OriginID[i] = message[i + StartIndexOriginID];
            }

            MessageText = message.Substring(StartIndexMessageText);
        }
        

    }
}
