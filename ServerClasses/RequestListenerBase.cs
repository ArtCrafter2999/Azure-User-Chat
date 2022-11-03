using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerDatabase;

namespace ServerClasses
{
    public abstract class RequestListenerBase : ClientModelBase
    {
        public override ServerEndpoint Endpoint { get => Client.Endpoint; set { Client.Endpoint = value; } }
        public override RequestResponseBase Respondent { get => Client.Respondent; set { Client.Respondent = value; } }
        public override RequestListenerBase Listener { get => Client.Listener; set { Client.Listener = value; } }
        public override ClientsNotifyerBase Notifyer { get => Client.Notifyer; set { Client.Notifyer = value; } }
        public override RequestHandlerBase Handler { get => Client.Handler; set { Client.Handler = value; } }

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

        public void Invoke(RequestType type, string raw)
        {
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
