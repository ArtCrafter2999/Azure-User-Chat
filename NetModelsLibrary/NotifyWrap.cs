using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary
{
    public class NotifyWrap
    {
        public NotifyType Type { get; set; }
        public string RawObject { get; set; }
        public NotifyWrap(NotifyType type, string raw)
        {
            Type = type;
            RawObject = raw;
        }
        public NotifyWrap()
        {

        }
    }
}
