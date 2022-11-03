using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BusSerializer = NetModelsLibrary.BusSerializer;

namespace ServerClasses
{
    public class ServerEndpoint
    {
        protected IConfigurationBuilder builder => new ConfigurationBuilder().AddJsonFile("appsettings.json");
        protected IConfigurationRoot configuration => builder.Build();
        public CancellationTokenSource TokenSource = new CancellationTokenSource();

        protected ServiceBusClient serviceBusClient => new ServiceBusClient(configuration["ConnectionString"]);
        public string SessionId { get; set; }
        public ServerEndpoint(string sessionId)
        {
            SessionId = sessionId;
        }
        private async Task Send<T>(T obj, string queue)
        {
            var sender = serviceBusClient.CreateSender(queue);
            var requestMsg = new ServiceBusMessage(BusSerializer.Serialize(obj));
            requestMsg.SessionId = SessionId;
            await sender.SendMessageAsync(requestMsg, TokenSource.Token);
        }
        public async Task SendReply<T>(T obj) => await Send(obj, configuration["ReplyQueueName"]);
        public async Task SendNotify<T>(T obj, NotifyType type)
        {
            var res = new NotifyWrap(type, BusSerializer.Serialize(obj));
            await Send(res, configuration["NotifyerQueueName"]);
        }
        
    }
}
