using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary
{
    public class RequestWrap
    {
        public RequestType Type { get; set; }
        public string ID { get; set; }
        public string? RawObject { get; set; }
        public RequestWrap(RequestType type, string session, string rawObject)
        {
            Type = type;
            ID = session;
            RawObject = rawObject;
        }
        public RequestWrap(RequestType type, string session)
        {
            Type = type;
            ID = session;
        }
        public RequestWrap()
        {

        }
    }
}
