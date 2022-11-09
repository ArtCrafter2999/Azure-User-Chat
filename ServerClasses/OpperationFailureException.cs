using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public class OperationFailureException : Exception
    {
        public RequestType? BusType { get; set; }
        public OperationFailureException(RequestType type, string message) : base(message) { BusType = type; }
        public OperationFailureException(string message) : base(message) { }
        public OperationFailureException() : base("не вдалося виконати цю операцію") { }
    }
}
