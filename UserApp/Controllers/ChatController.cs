﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserApp.Views;
using UserApp.Models;
using System.Windows;

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
        }

        public void LoadChats()
        {
            Connection.Network.WriteRequest(NetModelsLibrary.RequestType.GetAllChats);
            var allchats = Connection.Network.ReadObject<NetModelsLibrary.Models.AllChatsModel>();
            SelfUser = new UserModel(allchats.User);
            ChatModels.Clear();
            foreach (var NetChatModel in allchats.Chats)
            {
                ChatModels.Add(new ChatModel(NetChatModel));
            }
        }
        public NetModelsLibrary.Models.MessagesPageModel LoadMessages(int From)
        {
            if (!SelectedChatModel.IsEnd)
            {
                Connection.Network.WriteRequest(NetModelsLibrary.RequestType.GetPageOfMessages);
                Connection.Network.WriteObject(new NetModelsLibrary.Models.GetMessagesInfoModel()
                {
                    ChatId = SelectedChatModel.Id,
                    From = From
                });
                return Connection.Network.ReadObject<NetModelsLibrary.Models.MessagesPageModel>();
            }
            return new NetModelsLibrary.Models.MessagesPageModel() { Messages = new List<NetModelsLibrary.Models.MessageModel>(), From = From, To = From};
        }
        public void SendMessage(NetModelsLibrary.Models.MessageModel message)
        {
            Connection.Network.WriteRequest(NetModelsLibrary.RequestType.SendMessage);
            Connection.Network.WriteObject(message);
            MessageSended.Invoke(
                new MessageModel(
                    Connection.Network.ReadObject
                        <NetModelsLibrary.Models.MessageModel>()
                )
            );
        }
    }
}
