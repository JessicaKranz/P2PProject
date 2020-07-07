using System;
using System.Collections.Generic;
using System.Text;

namespace Datenmodelle
{
    public class GroupChat
    {

        public int groupChatID { get; set; }                                   
        public List<string> groupChatName { get; set; } = new List<string>();    

    }
}
