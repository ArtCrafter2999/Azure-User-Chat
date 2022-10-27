using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class UserCreationModel : BusTypeModel
    {
        public string Name { get; set; }
        public string PasswordMD5 { get; set; }
        public string Login { get; set; }
    }
}
