using Microsoft.Extensions.Logging;
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
        public virtual ClientBase Client { get; set; }
        public virtual ServerEndpoint Endpoint { get => Client.Endpoint; set { Client.Endpoint = value; } }
        public virtual RequestResponseBase Respondent { get => Client.Respondent; set { Client.Respondent = value; } }
        public virtual RequestListenerBase Listener { get => Client.Listener; set { Client.Listener = value; } }
        public virtual ClientsNotifyerBase Notifyer { get => Client.Notifyer; set { Client.Notifyer = value; } }
        public virtual RequestHandlerBase Handler { get => Client.Handler; set { Client.Handler = value; } }
        public virtual ILogger? Logger { get => Client.Logger; set => Client.Logger = value; }
    }
}
