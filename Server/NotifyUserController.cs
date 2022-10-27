using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class NotifyUserController
    {
        public Network network => Client.network;
        public ClientObject Client { get; set; }
        public DataBaseHandler Handler { get; set; }
        public NotifyUserController(ClientObject client, DataBaseHandler dataBaseHandler)
        {
            Client = client;
            Handler = dataBaseHandler;
        }

        public void ChatCreated(ChatModel model)
        {
            string? origTitle = model.Title;
            foreach (var user in model.Users)
            {
                if (user.Id != Handler.User.Id && IsUserOnline(user.Id))
                {
                    var userNetwork = DataBaseHandler.NetworkOfId[user.Id];
                    model.Type = BusType.ChatCreated;
                    model.Title = origTitle ?? Handler.GenerateChatName(model, user);
                    userNetwork.WriteObject(model);
                }
            }
        }
        private bool IsUserOnline(int userId)
        {
            return DataBaseHandler.UsersOnline.Contains(userId);
        }

        public void MessageSended(MessageModel message, Chat chat)
        {
            foreach (var user in chat.Users)
            {
                if (user.Id != Handler.User.Id)
                {
                    DataBaseHandler.AddUnreaded(chat.Id, user.Id);
                    if (IsUserOnline(user.Id))
                    {
                        var userNetwork = DataBaseHandler.NetworkOfId[user.Id];
                        message.Type = BusType.MessageSended;
                        userNetwork.WriteObject(message);
                    }
                }
                else
                {
                    message.Type = BusType.MessageSended;
                    Handler.network.WriteObject(message);
                }
            }
        }

        public void UserChangeStatus()
        {
            var rel = Handler.GetRelativeUsers();
            foreach (var user in rel)
            {
                if (IsUserOnline(user.Id))
                {
                    var net = DataBaseHandler.NetworkOfId[user.Id];
                    net.WriteObject(new UserStatusModel(Handler.User, IsUserOnline(Handler.User.Id)) { Type = BusType.UserChangeStatus });
                }
            }
        }

        public void ChatChanged(ChatModel model, List<User> added, List<User> removed, List<User> notChanged)
        {
            foreach (var user in added)
            {
                if (IsUserOnline(user.Id))
                {
                    var net = DataBaseHandler.NetworkOfId[user.Id];
                    model.Type = BusType.ChatCreated;
                    net.WriteObject(model);
                }
            }
            foreach (var user in removed)
            {
                if (IsUserOnline(user.Id))
                {
                    var net = DataBaseHandler.NetworkOfId[user.Id];
                    net.WriteObject(new IdModel(model.Id) { Type = BusType.ChatDeleted });
                }
            }
            foreach (var user in notChanged)
            {
                if (IsUserOnline(user.Id))
                {
                    var net = DataBaseHandler.NetworkOfId[user.Id];
                    model.Type = BusType.ChatChanged;
                    net.WriteObject(model);
                }
            }
        }
        public void ChatDeleted(IdModel id, List<User> users)
        {
            foreach (var user in users)
            {
                if (IsUserOnline(user.Id))
                {
                    var net = DataBaseHandler.NetworkOfId[user.Id];
                    id.Type = BusType.ChatDeleted;
                    net.WriteObject(id);
                }
            }
        }
    }
}
