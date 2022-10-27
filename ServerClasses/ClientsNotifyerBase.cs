using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public abstract class ClientsNotifyerBase : ClientModelBase
    {
        public override ServerEndpoint Endpoint { get => Client.Endpoint; set { Client.Endpoint = value; } }
        public override RequestResponseBase Respondent { get => Client.Respondent; set { Client.Respondent = value; } }
        public override RequestListenerBase Listener { get => Client.Listener; set { Client.Listener = value; } }
        public override ClientsNotifyerBase Notifyer { get => Client.Notifyer; set { Client.Notifyer = value; } }
        public override RequestHandlerBase Handler { get => Client.Handler; set { Client.Handler = value; } }

        public abstract void ChatCreated(Chat model);
        public abstract void MessageSended(Message message, Chat chat);
        public abstract void UserChangeStatus();
        public abstract void ChatChanged(Chat model, List<User> added, List<User> removed, List<User> notChanged);
        public abstract void ChatDeleted(int ChatId, List<User> users);
    }
}
