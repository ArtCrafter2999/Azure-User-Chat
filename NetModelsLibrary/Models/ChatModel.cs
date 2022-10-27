using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class ChatModel : BusTypeModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<UserStatusModel> Users { get; set; }
        public MessageModel? LastMessage { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? DateOfChange { get; set; }
        public int Unreaded { get; set; }
        public bool IsTrueTitle { get; set; }
        public ChatModel(){}
    }
}
