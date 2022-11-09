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
        public string sessionId { get; set; } = "";


        string _connectionString;
        string _replyQueueName;
        string _requestQueueName;
        string _notifyerQueueName;

        public ClientEndpoint()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            _connectionString = configuration["ConnectionString"];
            _replyQueueName = configuration["ReplyQueueName"];
            _requestQueueName = configuration["RequestQueueName"];
            _notifyerQueueName = configuration["NotifyerQueueName"];
        }
        public async Task SendRequest<T>(RequestType type, T obj)
        {
            await SendObject(new RequestWrap(type, sessionId, BusSerializer.Serialize(obj)));
        }
        private async Task<string> Receive(string QueueName)
        {
            await using (var serviceBusClient = new ServiceBusClient(_connectionString))
            {
                await using (var receiver = await serviceBusClient.AcceptSessionAsync(QueueName, sessionId, cancellationToken: Token))
                {
                    var replyMsg = await receiver.ReceiveMessageAsync(cancellationToken: Token);

                    if (replyMsg == null) throw new Exception("Failed to get reply from server");
                    var body = replyMsg.Body.ToString();
                    await receiver.CompleteMessageAsync(replyMsg);
                    return body;
                    throw new Exception("Failed to get reply from server");
                }
            }
        }
        public async Task<T> ReceiveReply<T>() => BusSerializer.Deserialize<T>(await Receive(_replyQueueName));
        public async Task<string> ReceiveReplyRaw() => await Receive(_replyQueueName);
        public async Task<NotifyWrap> ReceiveNotify() => BusSerializer.Deserialize<NotifyWrap>(await Receive(_notifyerQueueName));
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
                return new ResoultModel(RequestType.Auth, false, ex.Message);
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
                return new ResoultModel(RequestType.Registration, false, ex.Message); ;
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
            await using (var serviceBusClient = new ServiceBusClient(_connectionString))
            {
                await using (var sender = serviceBusClient.CreateSender(_requestQueueName))
                {
                    var body = BusSerializer.Serialize(obj);
                    var requestMsg = new ServiceBusMessage(body);
                    await sender.SendMessageAsync(requestMsg, Token);
                }
            }          
        }
    }
}
