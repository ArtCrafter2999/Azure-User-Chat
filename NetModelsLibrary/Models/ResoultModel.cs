using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary.Models
{
    public class ResoultModel 
    {
        public RequestType? BusType { get; set; } = null;
        public bool Success { get; set; }
        public string? Message { get; set; }

        public ResoultModel(){}
        public ResoultModel(bool success){ Success = success; }
        public ResoultModel(bool success, string message){ Success = success; Message = message; }
        public ResoultModel(RequestType? type, bool success, string message){ BusType = type; Success = success; Message = message; }
    }
}
