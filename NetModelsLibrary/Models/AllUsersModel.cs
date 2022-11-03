using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class AllUsersModel 
    {
        public List<UserStatusModel> Users { get; set; } = new List<UserStatusModel>();
    }
}
