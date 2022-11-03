using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class AllChatsModel 
    {
        public UserStatusModel User { get; set; }
        public List<ChatModel> Chats { get; set; }
    }
}
