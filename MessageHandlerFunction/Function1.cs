using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using NetModelsLibrary;
using NetModelsLibrary.Models;
using Microsoft.Extensions.Configuration;
using ServerClasses;
using System.Xml.Serialization;
using System.Threading;


namespace MessageHandlerFunction
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([ServiceBusTrigger("requests", Connection = "ConnectionString")]string message, ILogger log)
        {
            var type = Deserialize<RequestWrap>(message);
            log.LogInformation("Received " + type.Type);


            ClientFactory factory = new ClientFactory();
            factory.Client = new ClientObject();
            factory.Endpoint = new ServerEndpoint(type.ID);
            factory.Listener = new ServiceBusMessageListener();
            factory.Respondent = new RequestResponse();
            factory.Handler = new RequestHandler(factory.Listener);
            factory.Notifyer = new ClientsNotifyer();
            var client = factory.MakeClient();

            (client.Listener as ServiceBusMessageListener).Invoke(type);
            Thread.Sleep(10_000);
            client.Listener = null;
            client.Endpoint.TokenSource.Cancel();
            client.Endpoint = null;
            client.Handler = null;
            client.Notifyer = null;
            client.Respondent = null;
            client = null;
        }
        private static T Deserialize<T>(string s)
        {
            XmlSerializer serializer = new(typeof(T));
            using (TextReader tr = new StringReader(s))
            {
                T? res = (T?)serializer.Deserialize(tr);
                if (res == null) throw new Exception("Deserialization returned null");
                return res;
            }
        }
    }
}
