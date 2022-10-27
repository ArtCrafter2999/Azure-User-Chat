using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ServerClasses
{
    public abstract class ServerEndpoint
    {
        protected IConfigurationBuilder builder => new ConfigurationBuilder().AddJsonFile("appsettings.json");
        protected IConfigurationRoot configuration => builder.Build();

        protected ServiceBusClient serviceBusClient => new ServiceBusClient(configuration["ConnectionString"]);
        public abstract Task Send<T>(T obj);
    }

    public class ReplyEndpoint : ServerEndpoint
    {
        public string SessionId { get; set; }
        public ReplyEndpoint(string sessionId)
        {
            SessionId = sessionId;
        }
        public override async Task Send<T>(T obj)
        {
            var sender = serviceBusClient.CreateSender(configuration["ReplyQueueName"]);
            XmlSerializer serializer = new(typeof(T));
            MemoryStream temp = new();
            serializer.Serialize(temp, obj);

            var requestMsg = new ServiceBusMessage(new StreamReader(temp).ReadToEnd());
            requestMsg.SessionId = SessionId;
            await sender.SendMessageAsync(requestMsg);
        }
    }

    public class NotifyEndpoint : ServerEndpoint
    {
        public int IdTo { get; set; }
        public NotifyEndpoint(int idTo)
        {
            IdTo = idTo;
        }
        public override async Task Send<T>(T obj)
        {
            if (obj is BusTypeModel) (obj as BusTypeModel).ToUserId = IdTo;
            var sender = serviceBusClient.CreateSender(configuration["NotifyerQueueName"]);
            XmlSerializer serializer = new(typeof(T));
            MemoryStream temp = new();
            serializer.Serialize(temp, obj);

            var requestMsg = new ServiceBusMessage(new StreamReader(temp).ReadToEnd());
            await sender.SendMessageAsync(requestMsg);
        }
    }
}
