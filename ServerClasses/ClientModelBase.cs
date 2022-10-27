using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public abstract class ClientModelBase
    {
        public virtual ServerEndpoint Endpoint { get; set; }
        public virtual ClientBase Client { get; set; }
        public virtual RequestResponseBase Respondent { get; set; }
        public virtual RequestListenerBase Listener { get; set; }
        public virtual ClientsNotifyerBase Notifyer { get; set; }
        public virtual RequestHandlerBase Handler { get; set; }
    }
}
