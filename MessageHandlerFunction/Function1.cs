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

namespace MessageHandlerFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [ServiceBusTrigger(
            "requests",
            AutoCompleteMessages = false,
            Connection = "ConnectionString",
            IsSessionsEnabled = true)]
        ServiceBusReceivedMessage message,
        ILogger log)
        {
            ClientFactory factory = new ClientFactory();
            factory.Client = new ClientObject();
            factory.Endpoint = new ReplyEndpoint(message.SessionId);
            factory.Listener = new ServiceBusMessageListener();
            factory.Respondent = new RequestResponse();
            factory.Handler = new RequestHandler(factory.Listener);
            factory.Notifyer = new ClientsNotifyer();
            var client = factory.MakeClient();

            BusTypeModel type = Deserialize<BusTypeModel>(message.Body.ToString());

            log.LogInformation(type.Type.ToString() + " request");
            (client.Listener as ServiceBusMessageListener).Invoke(message);
            return new OkObjectResult(message);
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
