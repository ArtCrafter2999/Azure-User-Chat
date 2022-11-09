using Microsoft.Extensions.Logging;
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

        public abstract Task ChatCreated(Chat model);
        public abstract Task MessageSended(Message message, Chat chat);
        public abstract Task UserChangeStatus();
        public abstract Task ChatChanged(Chat model, List<User> added, List<User> removed, List<User> notChanged);
        public abstract Task ChatDeleted(int ChatId, List<User> users);
    }
}
