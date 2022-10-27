using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerDatabase;

namespace ServerClasses
{
    public abstract class RequestResponseBase : ClientModelBase
    {
        public override ServerEndpoint Endpoint { get => Client.Endpoint; set { Client.Endpoint = value; } }
        public override RequestResponseBase Respondent { get => Client.Respondent; set { Client.Respondent = value; } }
        public override RequestListenerBase Listener { get => Client.Listener; set { Client.Listener = value; } }
        public override ClientsNotifyerBase Notifyer { get => Client.Notifyer; set { Client.Notifyer = value; } }
        public override RequestHandlerBase Handler { get => Client.Handler; set { Client.Handler = value; } }

        public abstract void ResponseSuccess(BusType type, string message);
        public abstract void ResponseFailure(BusType type, string message);
        public abstract void ResponseChats(IEnumerable<Chat> chats);
        public abstract void ResponseUsers(IEnumerable<User> users);
        public abstract void ResponseMessagePage(int from, IEnumerable<Message> messages);
    }
}
