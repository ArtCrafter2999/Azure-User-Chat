using Microsoft.Extensions.Logging;
using NetModelsLibrary;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public abstract class ClientBase : ClientModelBase
    {
        public override ClientBase Client { get => this; set {} }
        private ServerEndpoint _endpoint;
        public override ServerEndpoint Endpoint { get => _endpoint; set { _endpoint = value; } }
        private RequestResponseBase _respondent;
        public override RequestResponseBase Respondent { get => _respondent; set { _respondent = value; } }
        private RequestListenerBase _listener;
        public override RequestListenerBase Listener { get => _listener; set { _listener = value; } }
        private ClientsNotifyerBase _notifyer;
        public override ClientsNotifyerBase Notifyer { get => _notifyer; set { _notifyer = value; } }
        private RequestHandlerBase _handler;
        public override RequestHandlerBase Handler { get => _handler; set { _handler = value; } }
        private ILogger? _logger;
        public override ILogger? Logger { get => _logger; set { _logger = value; } }

        public event Action<User> OnDisconected;

        public abstract ServerEndpoint GetOnlineUserEndpoint(int userId);
        public abstract bool IsUserOnline(int userId);
        public abstract void Disconect();
        public abstract void UserOnline();
        public abstract void UserOffline();

        public abstract void SetUser(int userId);

        public abstract User User { get; set; }
    }
}
