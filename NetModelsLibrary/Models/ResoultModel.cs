using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class ResoultModel : BusTypeModel
    {
        public BusType? BusType { get; set; } = null;
        public bool Success { get; set; }
        public string? Message { get; set; }

        public ResoultModel(){}
        public ResoultModel(bool success){ Success = success; }
        public ResoultModel(bool success, string message){ Success = success; Message = message; }
        public ResoultModel(BusType? type, bool success, string message){ BusType = type; Success = success; Message = message; }
    }
}
