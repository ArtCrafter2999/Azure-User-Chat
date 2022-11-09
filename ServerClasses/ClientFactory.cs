using Microsoft.Extensions.Logging;
using NetModelsLibrary;
using System.Net.Sockets;

namespace ServerClasses
{
    public class ClientFactory : ClientModelBase
    {
        private ClientBase _client;
        public override ClientBase Client { get => _client; set { _client = value; } }
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

        public ClientBase MakeClient()
        {

            if(Client == null) throw new Exception("Client не має значення");
            if(Respondent == null) throw new Exception("Respondent не має значення");
            if(Listener == null) throw new Exception("Listener не має значення");
            if(Notifyer == null) throw new Exception("Notifyer не має значення");
            if(Handler == null) throw new Exception("Handler не має значення");

            Client.Endpoint = Endpoint;
            Client.Client = Client;
            Client.Respondent = Respondent;
            Client.Listener = Listener;
            Client.Notifyer = Notifyer;
            Client.Logger = Logger;

            Respondent.Client = Client;
            Listener.Client = Client;
            Notifyer.Client = Client;
            Handler.Client = Client;

            return Client;
        }
    }
}