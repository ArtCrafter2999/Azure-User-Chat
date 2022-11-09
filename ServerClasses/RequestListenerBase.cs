using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerDatabase;

namespace ServerClasses
{
    public abstract class RequestListenerBase : ClientModelBase
    {

        public event Action<UserCreationModel> OnRegistration;
        public event Action<MessageModel> OnSendMessage;
        public event Action<AuthModel> OnAuth;
        public event Action OnGetAllChats;
        public event Action<ChatCreationModel> OnCreateChat;
        public event Action<SearchModel> OnSearchUsers;
        public event Action<GetMessagesInfoModel> OnGetPageOfMessages;
        public event Action<IdModel> OnReadUnreaded;
        public event Action<IdModel> OnMarkReaded;
        public event Action<ChatChangeModel> OnChangeChat;
        public event Action<IdModel> OnDeleteChat;
        public event Action OnLogOut;

        public void Invoke(RequestType type, string raw)
        {
            Logger?.LogInformation("Listener received request {0},\nraw: {1}", type, raw);
            switch (type)
            {
                case RequestType.Registration:
                    OnRegistration?.Invoke(BusSerializer.Deserialize<UserCreationModel>(raw));
                    break;
                case RequestType.SendMessage:
                    OnSendMessage?.Invoke(BusSerializer.Deserialize<MessageModel>(raw));
                    break;
                case RequestType.Auth:
                    OnAuth?.Invoke(BusSerializer.Deserialize<AuthModel>(raw));
                    break;
                case RequestType.GetAllChats:
                    OnGetAllChats?.Invoke();
                    break;
                case RequestType.CreateChat:
                    OnCreateChat?.Invoke(BusSerializer.Deserialize<ChatCreationModel>(raw));
                    break;
                case RequestType.SearchUsers:
                    OnSearchUsers?.Invoke(BusSerializer.Deserialize<SearchModel>(raw));
                    break;
                case RequestType.GetPageOfMessages:
                    OnGetPageOfMessages?.Invoke(BusSerializer.Deserialize<GetMessagesInfoModel>(raw));
                    break;
                case RequestType.ReadUnreaded:
                    OnReadUnreaded?.Invoke(BusSerializer.Deserialize<IdModel>(raw));
                    break;
                case RequestType.MarkReaded:
                    OnMarkReaded?.Invoke(BusSerializer.Deserialize<IdModel>(raw));
                    break;
                case RequestType.ChangeChat:
                    OnChangeChat?.Invoke(BusSerializer.Deserialize<ChatChangeModel>(raw));
                    break;
                case RequestType.DeleteChat:
                    OnDeleteChat?.Invoke(BusSerializer.Deserialize<IdModel>(raw));
                    break;
                default:
                    break;
            }
        }
    }
}
