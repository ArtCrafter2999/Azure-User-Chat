using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class GetMessagesInfoModel : BusTypeModel
    {
        public int ChatId { get; set; }
        //public int PageMessagesCount { get; set; }
        public int From { get; set; }
    }
}
