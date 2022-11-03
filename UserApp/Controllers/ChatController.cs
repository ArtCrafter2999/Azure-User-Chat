using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserApp.Views;
using UserApp.Models;
using System.Windows;
using NetModelsLibrary;

namespace UserApp.Controllers
{
    public class ChatController : INotifyProperyChangedBase
    {
        public List<ChatModel> ChatModels { get; set; } = new List<ChatModel>();
        public UserModel? SelfUser { get; set; }
        public ChatModel? SelectedChatModel { get => _selectedChatModel; set { _selectedChatModel = value; OnPropertyChanged(nameof(SelectedChatModel)); ChatChanged?.Invoke(); } }
        private ChatModel? _selectedChatModel = null;
        public ViewModels.OverlayGrid OverlayGrid => MainWindow.OverlayGrid;
        public ViewModels.ChatMessagesViewModel ChatView => MainWindow.ChatView;

        public event Action? ChatChanged;
        public event Action<MessageModel>? MessageSended;

        public MainWindow MainWindow => MainWindow.instance;
        public ReceiveNotifyController UpdateController;

        public ChatController()
        {
            UpdateController = new ReceiveNotifyController();
            MainWindow.instance.OverlayGrid.AuthView.Success += _ =>
            {
                UpdateController.Start();
            };
            ChatChanged += () =>
            {
                foreach (var model in ChatModels)
                {
                    model?.ChatView?.OnPropertyChanged("Color");
                }
            };
        }

        public async void LoadChatsAsync()
        {
            await Connection.Endpoint.SendObject(new RequestWrap(RequestType.GetAllChats, Connection.Endpoint.sessionId));
            var allchats = await Connection.Endpoint.ReceiveReply<NetModelsLibrary.Models.AllChatsModel>();
            SelfUser = new UserModel(allchats.User);
            ChatModels.Clear();
            foreach (var NetChatModel in allchats.Chats)
            {
                ChatModels.Add(new ChatModel(NetChatModel));
            }
        }
        public async Task<NetModelsLibrary.Models.MessagesPageModel> LoadMessagesAsync(int From)
        {
            if (!SelectedChatModel.IsEnd)
            {
                await Connection.Endpoint.SendRequest(RequestType.GetPageOfMessages, new NetModelsLibrary.Models.GetMessagesInfoModel()
                {
                    ChatId = SelectedChatModel.Id,
                    From = From,
                });
                return Connection.Endpoint.ReceiveReply<NetModelsLibrary.Models.MessagesPageModel>().Result;
            }
            return new NetModelsLibrary.Models.MessagesPageModel() { Messages = new List<NetModelsLibrary.Models.MessageModel>(), From = From, To = From};
        }
        public async void SendMessageAsync(NetModelsLibrary.Models.MessageModel message)
        {
            await Connection.Endpoint.SendRequest(RequestType.SendMessage, message);
            //MessageSended.Invoke(
            //    new MessageModel(
            //        Connection.Endpoint.ReceiveReply
            //            <NetModelsLibrary.Models.MessageModel>()
            //    )
            //);
            if (SelectedChatModel?.ChatView != null) SelectedChatModel.ChatView.Unreaded = 0;
        }
    }
}
