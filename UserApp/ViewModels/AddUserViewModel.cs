using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserApp.Models;
using Commands;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using NetModelsLibrary;

namespace UserApp.ViewModels
{
    public class AddUserViewModel : INotifyProperyChangedBase
    {
        public bool Visibility { get => _visibility; set { _visibility = value; OnPropertyChanged(nameof(Visibility)); } }
        private bool _visibility = false;

        public Views.MainWindow MainWindow => Views.MainWindow.instance;
        public ChatCreationViewModel ChatView => MainWindow.OverlayGrid.ChatCreationView;

        public NetModelsLibrary.Models.SearchModel SearchModel { get; set; } = new NetModelsLibrary.Models.SearchModel();

        public List<UserModel> Users { get; set; } = new List<UserModel>();

        public RelayCommand Add => new RelayCommand(o =>
        {
            if (!MainWindow.OverlayGrid.ChatCreationView.AddedUsers.Contains((UserModel)o))
            {
                ChatView.AddedUsers.Add((UserModel)o);
                ChatView.UpdateUsersView();
                UpdateUsersList();
            }
        });
        public DispatcherTimer timer { get; set; }
        public AddUserViewModel()
        {
            timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 700),
            };
            timer.Tick += (_, __) => { if (SearchModel.SearchString != null && SearchModel.SearchString != "") SearchRequestSendAsync(); };

        }
        public void UpdateUsersList()
        {
            bool HasHiddenUsers = false;
            var users = new List<UserModel>();
            
            foreach (var user in Users)
            {
                if (!ChatView.RelatedUsers.Contains(user) && !ChatView.AddedUsers.Contains(user))
                {
                    users.Add(user);
                }
                else
                {
                    HasHiddenUsers = true;
                }
            }
            MainWindow.FoundUsers.Children.Clear();
            if (users.Count == 0)
            {
                var Text = new TextBox()
                {
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold,
                    Background = Brushes.SteelBlue
                };
                if (HasHiddenUsers)
                {
                    Text.Text = "Результати співпадаючі пошуку вже присутні";
                    MainWindow.FoundUsers.Children.Add(Text);
                }
                else
                {
                    Text.Text = "Не знайдено результатів співпадаючих пошуку";
                    MainWindow.FoundUsers.Children.Add(Text);
                }
            }
            else
            {
                foreach (var user in users)
                {
                    var view = new Views.UserView(user) { Click = Add };
                    view.Button.CommandParameter = user;
                    MainWindow.FoundUsers.Children.Add(view);
                }
            }
        }
        private string _pastResult = "";
        public async void SearchRequestSendAsync()
        {
            if (SearchModel.SearchString != _pastResult)
            {
                _pastResult = SearchModel.SearchString;
                SearchModel.Type = BusType.SearchUsers;
                await Connection.Endpoint.SendRequest(SearchModel);
                var allusers = await Connection.Endpoint.ReceiveReply<NetModelsLibrary.Models.AllUsersModel>();
                Users.Clear();
                foreach (var user in allusers.Users)
                {
                    Users.Add(new UserModel(user));
                }
                MainWindow.Dispatcher.Invoke(UpdateUsersList);
            }
        }
    }
}
