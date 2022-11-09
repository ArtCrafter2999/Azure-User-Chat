using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerDatabase;
using NetModelsLibrary.Models;
using Microsoft.Extensions.Logging;

namespace ServerClasses
{
    public abstract class RequestResponseBase : ClientModelBase
    {
        public abstract Task ResponseSuccess(RequestType type, string message);
        public abstract Task ResponseFailure(RequestType type, string message);
        public abstract Task ResponseChats(IEnumerable<Chat> chats);
        public abstract Task ResponseUsers(IEnumerable<User> users);
        public abstract Task ResponseMessagePage(int from, IEnumerable<Message> messages);
        public abstract Task ResponseId(int id);
    }
}
