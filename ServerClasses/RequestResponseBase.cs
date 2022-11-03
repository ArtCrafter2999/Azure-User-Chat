using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerDatabase;
using NetModelsLibrary.Models;

namespace ServerClasses
{
    public abstract class RequestResponseBase : ClientModelBase
    {
        public override ServerEndpoint Endpoint { get => Client.Endpoint; set { Client.Endpoint = value; } }
        public override RequestResponseBase Respondent { get => Client.Respondent; set { Client.Respondent = value; } }
        public override RequestListenerBase Listener { get => Client.Listener; set { Client.Listener = value; } }
        public override ClientsNotifyerBase Notifyer { get => Client.Notifyer; set { Client.Notifyer = value; } }
        public override RequestHandlerBase Handler { get => Client.Handler; set { Client.Handler = value; } }

        public abstract Task ResponseSuccess(RequestType type, string message);
        public abstract Task ResponseFailure(RequestType type, string message);
        public abstract Task ResponseChats(IEnumerable<Chat> chats);
        public abstract Task ResponseUsers(IEnumerable<User> users);
        public abstract Task ResponseMessagePage(int from, IEnumerable<Message> messages);
        public abstract Task ResponseId(int id);
    }
}
