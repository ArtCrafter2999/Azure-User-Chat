using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Xml.Serialization;

namespace UserApp
{
    public class ClientEndpoint
    {
        public CancellationToken Token { get; set; }
        public string sessionId = Guid.NewGuid().ToString();
        IConfigurationBuilder builder => new ConfigurationBuilder().AddJsonFile("appsettings.json");
        IConfigurationRoot configuration => builder.Build();

        ServiceBusClient serviceBusClient => new ServiceBusClient(configuration["ConnectionString"]);
        public async Task SendRequest<T>(T obj)
        {
            var sender = serviceBusClient.CreateSender(configuration["RequestQueueName"]);
            XmlSerializer serializer = new(typeof(T));
            MemoryStream temp = new();
            serializer.Serialize(temp, obj);

            var requestMsg = new ServiceBusMessage(new StreamReader(temp).ReadToEnd());
            requestMsg.SessionId = sessionId;
            await sender.SendMessageAsync(requestMsg, Token);
        }
        private async Task<string> Receive(string QueueName)
        {
            var receiver = await serviceBusClient.AcceptSessionAsync(QueueName, sessionId, cancellationToken: Token);
            var replyMsg = await receiver.ReceiveMessageAsync(cancellationToken: Token);

            if (replyMsg == null) throw new Exception("Failed to get reply from server");
            return replyMsg.Body.ToString();
        }
        public async Task<T> ReceiveReply<T>() => Deserialize<T>(await Receive(configuration["ReplyQueueName"]));
        public async Task<string> ReceiveReply() => await Receive(configuration["ReplyQueueName"]);
        public async Task<string> ReceiveNotify() => await Receive(configuration["NotifyerQueueName"]);
        public static T Deserialize<T>(string s)
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
