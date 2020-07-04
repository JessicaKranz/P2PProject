using System;
using System.Collections.Generic;
using System.Text;

namespace Datenmodelle
{
    public class GroupChat
    {

        public int groupChatID { get; set; }                                   
        public List<Peer> groupChatMembers { get; set; } = new List<Peer>();

        public List<string> groupChatName { get; set; } = new List<string>();    

    }
}
