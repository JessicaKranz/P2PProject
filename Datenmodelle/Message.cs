using Newtonsoft.Json;
using System;
using System.Net;

namespace Datenmodelle
{
    public class Message
    {
        public enum Types
        {
            PeerEntry,
            PeerJoin,
            PersonalMessage,
            GroupMessage,
            FishTank,
            WannabeNeighbour,

            //These Are Jessies
            JoinRequest,
            JoinResponse
        }
        /// <summary>
        /// Type of Message characterizes each Message
        /// </summary>
        public Types Type { get; set; }
        public IP Destination { get; set; } //I Question These
        public IP Source { get; set; }      // I want to bargain about this
        /// <summary>
        /// 
        /// </summary>
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
        /// senders name
        /// </summary>
        public string AuthorName { get; set; }
        /// <summary>
        /// Name of the GroupChat a Message is either a) broadcasting its existence or b) delivering a Message from. 
        /// </summary>
        public string GroupChatName { get; set; }
        /// <summary>
        /// Chat Message for either a specific Peer or a Group Chat (Depending on the DestinationId)
        /// </summary>
        public string ChatMessage { get; set; }
        /// <summary>
        /// TimeStamp of when the Message was Sent
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// A fish that is passed around in the network, to approximate the number of Peers connected.
        /// </summary>
        public Fish Fish { get; set; }
        /// <summary>
        /// IP Address from the Sender of the Original Message
        /// </summary>
        public IPAddress SendersIP { get; set; }
        /// <summary>
        /// The Number of neighbours a joining Peer to the Overlay wants to have.
        /// </summary>
        public int WishedNeighbours { get; set; }

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
