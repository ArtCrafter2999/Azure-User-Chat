using NetModelsLibrary;
using System.Net.Sockets;

namespace ServerClasses
{
    public class ClientFactory : ClientModelBase
    {
        public ClientBase MakeClient()
        {
            var list = new List<ClientModelBase>();
            list.Add(Client ?? throw new Exception("Client не має значення"));
            list.Add(Respondent ?? throw new Exception("Respondent не має значення"));
            list.Add(Listener ?? throw new Exception("Listener не має значення"));
            list.Add(Notifyer ?? throw new Exception("Notifyer не має значення"));
            list.Add(Handler ?? throw new Exception("Handler не має значення"));

            Client.Endpoint = Endpoint;
            Client.Client = Client;
            Client.Respondent = Respondent;
            Client.Listener = Listener;
            Client.Notifyer = Notifyer;

            Respondent.Client = Client;
            Listener.Client = Client;
            Notifyer.Client = Client;
            Handler.Client = Client;

            return Client;
        }
    }
}