using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class UserStatusModel 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastOnline { get; set; }
        public UserStatusModel(){}
        public UserStatusModel(ServerDatabase.User user, bool isOnline)
        {
            Id = user.Id;
            Name = user.Name;
            Login = user.Login;
            LastOnline = user.LastOnline;
            IsOnline = isOnline;
        }
    }
}
