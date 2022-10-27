using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public class RequestResponse : RequestResponseBase
    {
        public ClientBase Client { get; set; }
        public RequestResponseBase Respondent { get => Client.Respondent; set { Client.Respondent = value; } }
        public RequestListenerBase Listener { get => Client.Listener; set { Client.Listener = value; } }
        public ClientsNotifyerBase Notifyer { get => Client.Notifyer; set { Client.Notifyer = value; } }
        public RequestHandlerBase Handler { get => Client.Handler; set { Client.Handler = value; } }
        public ServerEndpoint Endpoint { get => Client.Endpoint; set { Client.Endpoint = value; } }

        public event Action<BusType, string> OnSuccess;
        public event Action<BusType, string> OnFailure;

        public string GenerateChatName(Chat chat)
        {
            var users = from User user in chat.Users
                        where user.Id != Client.User.Id
                        select user;
            var names = new List<string>();
            foreach (var user in users)
            {
                names.Add(user.Name);
            }
            return string.Join(", ", names);
        }
        public string GenerateChatName(ChatModel chat, UserStatusModel userexclusive)
        {
            var users = from UserStatusModel user in chat.Users
                        where user.Id != userexclusive.Id
                        select user;
            var names = new List<string>();
            foreach (var user in users)
            {
                names.Add(user.Name);
            }
            return string.Join(", ", names);
        }

        public override void ResponseChats(IEnumerable<Chat> chats)
        {
            AllChatsModel res = new AllChatsModel();
            res.User = new UserStatusModel()
            {
                Id = Client.User.Id,
                Login = Client.User.Login,
                Name = Client.User.Name,
                IsOnline = true,
                LastOnline = null
            };
            res.Chats = new List<ChatModel>();
            foreach (var chat in chats)
            {

                var userStatusModels = new List<UserStatusModel>();
                if (chat.Users != null)
                {
                    foreach (var user in chat.Users)
                    {
                        userStatusModels.Add(new UserStatusModel(user, Client.IsUserOnline(user.Id)));
                    }
                }

                MessageModel? messageModel = null;
                try
                {
                    var message = chat.Messages.OrderByDescending(c => c.SendTime).First();
                    if (message != null)
                    {
                        User user = chat.Users.First(o => o.Id == message.UserId);
                        if (user != null)
                            messageModel = new MessageModel(message, Client.IsUserOnline(user.Id)); // повідомлення, частина якого буде відображатися під чатом, як останне
                    }
                }
                catch (Exception) { }

                res.Chats.Add(new ChatModel()
                {
                    Id = chat.Id,
                    Title = chat.Title ?? GenerateChatName(chat),
                    IsTrueTitle = chat.Title != null,
                    Users = userStatusModels,
                    LastMessage = messageModel,
                    CreationDate = chat.CreationDate,
                    DateOfChange = chat.DateOfChange,
                    Unreaded = chat.UserChatRelatives.Find(ucr => ucr.User.Id == Client.User.Id).Unreaded
                }); ;
            }
            Endpoint.Send(res);
        }

        public override void ResponseFailure(BusType type, string message)
        {
            OnFailure?.Invoke(type, message);
            Endpoint.Send(new ResoultModel(type, false, message));
        }
        public override void ResponseSuccess(BusType type, string message)
        {
            OnSuccess?.Invoke(type, message);
            Endpoint.Send(new ResoultModel(type, true, message) {ToUserId = Client.User.Id});
        }

        public override void ResponseMessagePage(int from, IEnumerable<Message> messages)
        {
            var page = new MessagesPageModel()
            {
                From = from
            };
            var count = messages.Count();
            if (count < ClientObject.PageSize) page.IsEnd = true;
            page.To = from + count;
            foreach (var message in messages)
            {
                page.Messages.Add(new MessageModel(message, Client.IsUserOnline(message.User.Id)));
            }
            Endpoint.Send(page);
        }

        public override void ResponseUsers(IEnumerable<User> users)
        {
            var res = new AllUsersModel();
            foreach (var user in users)
            {
                res.Users.Add(new UserStatusModel(user, Client.IsUserOnline(user.Id)));
            }
            Endpoint.Send(res);
        }
    }
}
