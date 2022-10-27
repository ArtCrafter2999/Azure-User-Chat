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

        public void Invoke(string raw)
        {
            if (raw == null || raw == "") throw new Exception("received null");

            BusTypeModel infoModel = Deserialize<BusTypeModel>(raw);

            if (infoModel == null) throw new Exception("received null");
            if (infoModel.FromUserId.HasValue) Client.SetUser(infoModel.FromUserId.Value);

            switch (infoModel.Type)
            {
                case BusType.Registration:
                    OnRegistration?.Invoke(Deserialize<UserCreationModel>(raw));
                    break;
                case BusType.SendMessage:
                    OnSendMessage?.Invoke(Deserialize<MessageModel>(raw));
                    break;
                case BusType.Auth:
                    OnAuth?.Invoke(Deserialize<AuthModel>(raw));
                    break;
                case BusType.GetAllChats:
                    OnGetAllChats?.Invoke();
                    break;
                case BusType.CreateChat:
                    OnCreateChat?.Invoke(Deserialize<ChatCreationModel>(raw));
                    break;
                case BusType.SearchUsers:
                    OnSearchUsers?.Invoke(Deserialize<SearchModel>(raw));
                    break;
                case BusType.GetPageOfMessages:
                    OnGetPageOfMessages?.Invoke(Deserialize<GetMessagesInfoModel>(raw));
                    break;
                case BusType.ReadUnreaded:
                    OnReadUnreaded?.Invoke(Deserialize<IdModel>(raw));
                    break;
                case BusType.MarkReaded:
                    OnMarkReaded?.Invoke(Deserialize<IdModel>(raw));
                    break;
                case BusType.ChangeChat:
                    OnChangeChat?.Invoke(Deserialize<ChatChangeModel>(raw));
                    break;
                case BusType.DeleteChat:
                    OnDeleteChat?.Invoke(Deserialize<IdModel>(raw));
                    break;
                default:
                    break;
            }
        }
        private T Deserialize<T>(string s)
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
