using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using NetModelsLibrary;
using NetModelsLibrary.Models;
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
        public string sessionId { get; set; }
        IConfigurationBuilder builder => new ConfigurationBuilder().AddJsonFile("appsettings.json");
        IConfigurationRoot configuration => builder.Build();

        ServiceBusClient serviceBusClient => new ServiceBusClient(configuration["ConnectionString"]);
        public async Task SendRequest<T>(RequestType type, T obj)
        {
            await SendObject(new RequestWrap(type, sessionId, BusSerializer.Serialize(obj)));
        }
        private async Task<string> Receive(string QueueName)
        {
            var receiver = await serviceBusClient.AcceptSessionAsync(QueueName, sessionId, cancellationToken: Token);
            var replyMsg = await receiver.ReceiveMessageAsync(cancellationToken: Token);

            if (replyMsg == null) throw new Exception("Failed to get reply from server");
            return replyMsg.Body.ToString();
        }
        public async Task<T> ReceiveReply<T>() => BusSerializer.Deserialize<T>(await Receive(configuration["ReplyQueueName"]));
        public async Task<string> ReceiveReplyRaw() => await Receive(configuration["ReplyQueueName"]);
        public async Task<NotifyWrap> ReceiveNotify() => BusSerializer.Deserialize<NotifyWrap>(await Receive(configuration["NotifyerQueueName"]));
        public async Task<ResoultModel> Authorize(string Login, string Password)
        {
            try
            {
                var model = new AuthModel()
                {
                    Login = Login,
                    PasswordMD5 = CreateMD5(Password)
                };
                sessionId = Guid.NewGuid().ToString();
                await SendRequest(RequestType.Auth, model);
                var idmodel = await ReceiveReply<IdModel>();
                if (idmodel.Id == -1) throw new Exception("Не правельний логін або пароль");
                sessionId = idmodel.Id.ToString();
                return new ResoultModel(RequestType.Auth, true, "Authentification successful");
            }
            catch (Exception ex)
            {
                return new ResoultModel(RequestType.Auth, true, ex.Message);
            }
        }
        public async Task<ResoultModel> Register(string Login, string Password, string Name)
        {
            try
            {
                var model = new UserCreationModel()
                {
                    Login = Login,
                    Name = Name,
                    PasswordMD5 = CreateMD5(Password)
                };
                sessionId = Guid.NewGuid().ToString();
                await SendRequest(RequestType.Registration, model);
                var idmodel = await ReceiveReply<IdModel>();//стопнулося тут
                if (idmodel.Id == -1) throw new Exception("Такий користувач вже існує");
                sessionId = idmodel.Id.ToString();
                return new ResoultModel(RequestType.Registration, true, "Registration successful");
            }
            catch (Exception ex)
            {
                return new ResoultModel(RequestType.Registration, true, ex.Message);
            }
        }
        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public async Task SendObject<T>(T obj)
        {
            var sender = serviceBusClient.CreateSender(configuration["RequestQueueName"]);

            var body = BusSerializer.Serialize(obj);
            var requestMsg = new ServiceBusMessage(body);
            requestMsg.SessionId = sessionId;
            await sender.SendMessageAsync(requestMsg, Token);
        }
    }
}
