using Newtonsoft.Json;
using System;

namespace Datenmodelle
{
    public class Message
    {
        public enum Types
        {
            PersonalMessage,
            JoinRequest,
            JoinResponse,
            JoinAcknowledge,
            KillConnection
        }
        /// <summary>
        /// Type of Message characterizes each Message
        /// </summary>
        public Types Type { get; set; }
        public IP DestinationIP { get; set; } //I Question These
        public IP SourceIP { get; set; }      // I want to bargain about this

        public int Ttl { get; set; }
        /// <summary>
        /// target ID
        /// </summary>
        public int DestinationId { get; set; }
        /// <summary>
        /// senders ID
        /// </summary>
        public int SourceId { get; set; }
        /// <summary>
        /// If we ever check weather or not we will send a message back to the Peer we got a message from . This is the Attribute to compare with
        /// </summary>
        public int JoiningId { get; set; }
        /// <summary>
        /// senders ID
        /// </summary>
        public string ChatMessage { get; set; }
        /// <summary>
        /// TimeStamp of when the Message was Sent
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// A fish that is passed around in the network, to approximate the number of Peers connected.
        /// </summary>


        public Message()
        {
            TimeStamp = DateTime.Now;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
