using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserApp.Controllers;
using UserApp.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Commands;
using UserApp.ViewModels;
using System.Configuration;

namespace UserApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static MainWindow instance { get; set; }

        public ChatController ChatController { get; set; }

        public OverlayGrid OverlayGrid { get; set; }
        public ChatMessagesViewModel ChatView { get; set; }

        public ICommand CreateChat => new RelayCommand(o =>
        {
            OverlayGrid.Visibility = true;
            OverlayGrid.ChatCreationView.Visibility = true;
            OverlayGrid.ChatCreationView.UpdateUsersView();
        });
        public ICommand ChangeChat => new RelayCommand(o =>
        {
            if (ChatController.SelectedChatModel != null)
            {
                OverlayGrid.Visibility = true;
                OverlayGrid.ChatCreationView.Visibility = true;
                OverlayGrid.ChatCreationView.ChatCreationModel.Title = 
                    ChatController.SelectedChatModel.IsTrueTitle ? 
                        ChatController.SelectedChatModel.Title :
                            null;
                OverlayGrid.ChatCreationView.AddedUsers = new List<UserModel>(from UserModel u in ChatController.SelectedChatModel.Users
                                                                              where u.Id != ChatController.SelfUser.Id select u);
                OverlayGrid.ChatCreationView.UpdateUsersView();
                OverlayGrid.ChatCreationView.TurnChangeMode(ChatController.SelectedChatModel.Id);
            }
        });

        public MainWindow()
        {
            instance = this;

            InitializeComponent();
            DataContext = this;

            OverlayGrid = new OverlayGrid();
            ChatController = new ChatController();
            ChatView = new ChatMessagesViewModel();


            //ChatController.MessageSended += m =>
            //{
            //    if (m.Chat != null)
            //    {
            //        m.Chat.LastMessage = m;
            //        SortChats();
            //    }
            //};

            OverlayGrid.AuthView.Success += _ => UpdateChatView();
        }

        public async void UpdateChatView()
        {
            await ChatController.LoadChatsAsync();
            SortChats();
            OverlayGrid.HideAll();
        }

        public void SortChats()
        {
            ChatsStack.Children.Clear();
            ChatController.ChatModels.Sort((a, b) => a.LastTime < b.LastTime ? 1 : -1);
            foreach (var model in ChatController.ChatModels)
            {
                var view = new ChatView(model);
                model.ChatView = view;
                ChatsStack.Children.Add(view);
                view.OnPropertyChanged(nameof(view.Color));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void EnterMoveNextFocus(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var control = sender as Control;
                AuthLogin.Focus();
                if (control == AuthLogin) AuthPassword.Focus();
                if (control == AuthPassword) if (AuthBtn.Command.CanExecute(AuthPassword)) 
                        AuthBtn.Command.Execute(AuthPassword);
                if (control == RegLogin) RegPassword.Focus();
                if (control == RegPassword) RegName.Focus();
                if (control == RegName) if (RegBtn.Command.CanExecute(RegPassword)) 
                        RegBtn.Command.Execute(RegPassword);
            }
            if (e.Key == Key.Tab)
            {

            }
        }

        private void ChatTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var text = (sender as TextBox).Text.Trim();
            if (e.Key == Key.Return && text.Length > 0)
            {
                (sender as TextBox).Text = "";
                ChatController.SendMessageAsync(new NetModelsLibrary.Models.MessageModel()
                {
                    ChatId = ChatController.SelectedChatModel.Id,
                    User = new NetModelsLibrary.Models.UserStatusModel()
                    {
                        Id = ChatController.SelfUser.Id,
                        Login = ChatController.SelfUser.Login,
                        Name = ChatController.SelfUser.Name,
                        IsOnline = true,
                        LastOnline = ChatController.SelfUser.LastOnline,
                    },
                    SendTime = DateTime.Now,
                    Text = text
                });
            }
        }
    }
}
