using UserApp.Models;
using UserApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commands;
using System.Windows.Input;
using System.Windows.Controls;
using NetModelsLibrary;

namespace UserApp.ViewModels
{
    public class ChatCreationViewModel : INotifyProperyChangedBase
    {
        public bool Visibility { get => _visibility; set { _visibility = value; OnPropertyChanged(nameof(Visibility)); } }
        private bool _visibility = false;

        public AddUserViewModel AddUserView { get; set; }

        public MainWindow MainWindow => MainWindow.instance;

        public NetModelsLibrary.Models.ChatCreationModel ChatCreationModel { get; set; } = new NetModelsLibrary.Models.ChatCreationModel();
        public NetModelsLibrary.Models.ChatChangeModel ChatChangeModel { get; set; } = new NetModelsLibrary.Models.ChatChangeModel();

        public List<UserModel> RelatedUsers { get; set; } = new List<UserModel>();
        public List<UserModel> AddedUsers { get; set; } = new List<UserModel>();

        public event Action ChatCreated;

        public ICommand Create => new RelayCommand(async o =>
        {
            ChatCreationModel.Users = new List<NetModelsLibrary.Models.IdModel>();
            foreach (var user in AddedUsers)
            {
                ChatCreationModel.Users.Add(new NetModelsLibrary.Models.IdModel(user.Id));
            }
            ChatCreationModel.Type = BusType.CreateChat;
            await Connection.Endpoint.SendRequest(ChatCreationModel);
            if ((await Connection.Endpoint.ReceiveReply<NetModelsLibrary.Models.ResoultModel>()).Success) ChatCreated?.Invoke();

            MainWindow.OverlayGrid.HideAll();
            AddedUsers.Clear();
            ChatCreationModel = new NetModelsLibrary.Models.ChatCreationModel();
            AddUserView.Users.Clear();
            AddUserView.SearchModel.SearchString = "";
            AddUserView.timer.Stop();
            AddUserView.OnPropertyChanged(nameof(AddUserView.SearchModel));

            TurnOffChangeMode();
        }, o => AddedUsers.Count > 0);

        public ICommand AddUser => new RelayCommand(o =>
        {
            if (AddUserView.Visibility == false)
            {
                AddUserView.Visibility = true;
                AddUserView.timer.Start();
            }
        });
        public ICommand Cancel => new RelayCommand(o =>
        {
            MainWindow.OverlayGrid.HideAll();
            AddedUsers.Clear();
            ChatCreationModel = new NetModelsLibrary.Models.ChatCreationModel();
            AddUserView.Users.Clear();
            AddUserView.SearchModel.SearchString = "";
            AddUserView.timer.Stop();
            AddUserView.OnPropertyChanged(nameof(AddUserView.SearchModel));
            TurnOffChangeMode();
        });
        public ICommand Change => new RelayCommand(o =>
        {
            ChatChangeModel.Users = new List<NetModelsLibrary.Models.IdModel>();
            ChatChangeModel.Title = ChatCreationModel.Title;
            foreach (var user in AddedUsers)
            {
                ChatChangeModel.Users.Add(new NetModelsLibrary.Models.IdModel(user.Id));
            }
            ChatChangeModel.Type = BusType.ChangeChat;
            Connection.Endpoint.SendRequest(ChatChangeModel);

            MainWindow.ChatController.SelectedChatModel = null;
            MainWindow.OverlayGrid.HideAll();
            AddedUsers.Clear();
            ChatCreationModel = new NetModelsLibrary.Models.ChatCreationModel();
            AddUserView.Users.Clear();
            AddUserView.SearchModel.SearchString = "";
            AddUserView.timer.Stop();
            AddUserView.OnPropertyChanged(nameof(AddUserView.SearchModel));
            TurnOffChangeMode();
        }, o => AddedUsers.Count > 0);
        public ICommand Delete => new RelayCommand(o =>
        {
            Connection.Endpoint.SendRequest(new NetModelsLibrary.Models.IdModel(ChatChangeModel.Id) { Type = BusType.DeleteChat });

            MainWindow.ChatController.SelectedChatModel = null;
            MainWindow.OverlayGrid.HideAll();
            AddedUsers.Clear();
            ChatCreationModel = new NetModelsLibrary.Models.ChatCreationModel();
            AddUserView.Users.Clear();
            AddUserView.SearchModel.SearchString = "";
            AddUserView.timer.Stop();
            AddUserView.OnPropertyChanged(nameof(AddUserView.SearchModel));
            TurnOffChangeMode();
        });

        public ChatCreationViewModel()
        {
            AddUserView = new AddUserViewModel();
            ChatCreated += () => { MainWindow.instance.ChatController.SelectedChatModel = null; MainWindow.UpdateChatView(); };
            TurnOffChangeMode();
        }

        public void TurnChangeMode(int id)
        {
            ChatChangeModel.Id = id;
            MainWindow.ChangeButtons.Visibility = System.Windows.Visibility.Visible;
            MainWindow.CreationButtons.Visibility = System.Windows.Visibility.Hidden;
            MainWindow.CenterGroupBox.Header = "Змінення чату";
            OnPropertyChanged(nameof(ChatCreationModel));
        }
        public void TurnOffChangeMode()
        {
            MainWindow.ChangeButtons.Visibility = System.Windows.Visibility.Hidden;
            MainWindow.CreationButtons.Visibility = System.Windows.Visibility.Visible;
            MainWindow.CenterGroupBox.Header = "Створення нового чату";
            OnPropertyChanged(nameof(ChatCreationModel));
        }

        public void UpdateUsersView()
        {
            RelatedUsers.Clear();
            foreach (var chat in MainWindow.ChatController.ChatModels)
            {
                foreach (var user in chat.Users)
                {
                    if (!AddedUsers.Contains(user) && !RelatedUsers.Contains(user) && MainWindow.ChatController.SelfUser.Id != user.Id) RelatedUsers.Add(user);
                }
            }
            RelatedUsers.Sort((a, b) => string.Compare(a.Name, b.Name));

            MainWindow.AddUsersInChat.Children.Clear();
            MainWindow.AddedUsersInChat.Children.Clear();

            MainWindow.AddUsersInChat.Children.Add(new Button()
            {
                FontSize = 17,
                Content = "+ Додати нового",
                Background = System.Windows.Media.Brushes.White,
                Foreground = System.Windows.Media.Brushes.SteelBlue,
                Command = AddUser
            });
            foreach (var user in RelatedUsers)
            {
                MainWindow.AddUsersInChat.Children.Add(new UserView(user) 
                {
                    Click = new RelayCommand(o =>
                    {
                        if (!AddedUsers.Contains(user)) AddedUsers.Add(user);
                        UpdateUsersView();
                    })
               });
            }
            foreach (var user in AddedUsers)
            {
                MainWindow.AddedUsersInChat.Children.Add(new UserView(user)
                {
                    Click = new RelayCommand(o =>
                    {
                        AddedUsers.Remove(user);
                        AddUserView.UpdateUsersList();
                        UpdateUsersView();
                    })
                });
            }
        }
        //private void NotAlreadyContained(UserModel user)
        //{

        //}
    }
}
