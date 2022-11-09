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
        public void Run([ServiceBusTrigger("requests", Connection = "ConnectionString")] string message, ILogger log)
        {
            var type = BusSerializer.Deserialize<RequestWrap>(message);
            log.LogInformation("Received " + type.Type + "   raw:" + type.RawObject + "    ID:" + type.ID);

            try
            {
                log.LogInformation("Try started");
                ClientFactory factory = new ClientFactory();
                log.LogInformation("Factory created");
                factory.Client = new ClientObject();
                log.LogInformation("Client created");
                factory.Endpoint = new ServerEndpoint(type.ID);
                log.LogInformation("Endpoint created");
                factory.Listener = new ServiceBusMessageListener();
                log.LogInformation("Listener created");
                factory.Respondent = new RequestResponse();
                log.LogInformation("Respondent created");
                factory.Handler = new RequestHandler(factory.Listener);
                log.LogInformation("Handler created");
                factory.Notifyer = new ClientsNotifyer();
                log.LogInformation("Notifyer created");
                factory.Logger = log;

                var client = factory.MakeClient();
                log.LogInformation("client maked");

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
            catch (Exception ex)
            {
                log.LogError("Root Exception: {0},   stacktrace: {1}", ex.Message, ex.StackTrace);
            }

        }
    }
}
