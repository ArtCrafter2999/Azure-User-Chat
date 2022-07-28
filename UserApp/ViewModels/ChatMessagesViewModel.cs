﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserApp.Controllers;
using UserApp.Views;
using UserApp.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace UserApp.ViewModels
{
    public class ChatMessagesViewModel : INotifyProperyChangedBase
    {
        public MainWindow MainWindow => MainWindow.instance;
        public ChatController Controller => MainWindow.ChatController;
        public ChatModel? selectedChat => Controller.SelectedChatModel;
        public bool IsSelected => Controller.SelectedChatModel != null;



        public ChatMessagesViewModel()
        {
            Controller.ChatChanged += ChatChanged;
            Controller.MessageSended += MessageDown;
            MainWindow.MessageScroll.ScrollChanged += CheckTop;
        }

        private double prevPos = 0;
        private bool scrollToPrev = false;
        private void CheckTop(object sender, ScrollChangedEventArgs e)
        {
            if (scrollToPrev)
            {
                MainWindow.MessageScroll.ScrollToVerticalOffset(MainWindow.MessageScroll.ScrollableHeight - prevPos);
                scrollToPrev = false;
            }
            else if (selectedChat != null && e.VerticalChange < 0 && e.VerticalOffset == 0 && !selectedChat.IsEnd)
            {
                if (!selectedChat.IsEnd)
                {
                    MessagesUp(LoadMessages());
                    scrollToPrev = true;
                }
            }
            else
            {
                prevPos = MainWindow.MessageScroll.ScrollableHeight;
            }
        }
        public List<MessageModel> LoadMessages()
        {
            if (!selectedChat.IsEnd)
            {
                var messagespage = Controller.LoadMessages(selectedChat.Loaded);
                selectedChat.Loaded += messagespage.To;
                selectedChat.IsEnd = messagespage.IsEnd;
                var newmessages = new List<MessageModel>();
                foreach (var message in messagespage.Messages)
                {
                    selectedChat.Messages.Insert(0, new MessageModel(message));
                    newmessages.Add(new MessageModel(message));
                }
                newmessages.Reverse();
                return newmessages;
            }
            return new List<MessageModel>();
        }

        public void MessagesUp(List<MessageModel> NewMessages)
        {
            int index = 0;
            foreach (var message in NewMessages)
            {
                var view = new MessageView(message);
                MainWindow.MessagesStack.Children.Insert(index++, view);
            }
            MainWindow.MessageScroll.ScrollToVerticalOffset(1);
        }

        public void MessagesDown(List<MessageModel> NewMessages)
        {
            foreach (var message in NewMessages)
            {
                MainWindow.MessagesStack.Children.Add(new MessageView(message));
            }
        }
        public void MessagesDownWithHalfScroll(List<MessageModel> NewMessages)
        {
            foreach (var message in NewMessages)
            {
                MainWindow.MessagesStack.Children.Add(new MessageView(message));
            }
            MainWindow.MessageScroll.ScrollToVerticalOffset(MainWindow.MessageScroll.ViewportHeight * 1.5 + MainWindow.MessageScroll.ContentVerticalOffset);
        }
        public void MessagesDownWithScroll(List<MessageModel> NewMessages)
        {
            foreach (var message in NewMessages)
            {
                MainWindow.MessagesStack.Children.Add(new MessageView(message));
            }

            MainWindow.MessageScroll.ScrollToEnd();
            MainWindow.MessageScroll.UpdateLayout();
        }

        public void MessageDown(MessageModel message)
        {
            bool scroll = false;
            if (MainWindow.MessageScroll.VerticalOffset == MainWindow.MessageScroll.ScrollableHeight) scroll = true;
            MainWindow.MessagesStack.Children.Add(new MessageView(message));
            if (scroll)
            {
                MainWindow.MessageScroll.ScrollToEnd();
                MainWindow.MessageScroll.UpdateLayout();
            }
        }
        public void ChatChanged()
        {
            OnPropertyChanged(nameof(IsSelected));
            MainWindow.MessagesStack.Children.Clear();
            if (selectedChat != null)
            {
                if (selectedChat.UnreadedMessageCount > 0)
                {
                    ReadUnreaded();
                }
                else if (selectedChat.Loaded == 0)
                {
                    MessagesDownWithScroll(LoadMessages());
                }
                else
                {
                    MessagesDownWithScroll(selectedChat.Messages);
                }
            }
            else
            {
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private void ReadUnreaded()
        {
            if (selectedChat?.Messages != null && Controller.SelectedChatModel.Messages.Count == 0)
            {
                Connection.Network.WriteRequest(NetModelsLibrary.RequestType.ReadUnreaded);
                Connection.Network.WriteObject(new NetModelsLibrary.Models.IdModel(selectedChat.Id));
                var page = Connection.Network.ReadObject<NetModelsLibrary.Models.MessagesPageModel>();
                selectedChat.ChatView.Unreaded = 0;

                selectedChat.Loaded = page.To;
                var list = new List<MessageModel>();
                foreach (var message in page.Messages)
                {
                    list.Add(new MessageModel(message));
                    selectedChat.Messages.Insert(0, new MessageModel(message));
                }
                list.Reverse();
                MessagesDownWithHalfScroll(list);
            }
            else
            {
                Connection.Network.WriteRequest(NetModelsLibrary.RequestType.MarkReaded);
                Connection.Network.WriteObject(new NetModelsLibrary.Models.IdModel(selectedChat.Id));
                selectedChat.ChatView.Unreaded = 0;
                MessagesDownWithHalfScroll(selectedChat.Messages);
            }
        }
    }
}
