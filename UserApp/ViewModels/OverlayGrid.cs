using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Commands;
using UserApp.Controllers;

namespace UserApp.ViewModels
{
    public class OverlayGrid : INotifyProperyChangedBase
    {
        public bool Visibility { get => _visibility; set { _visibility = value; OnPropertyChanged(nameof(Visibility)); } }
        private bool _visibility = true;

        public ResoultViewModel ResoultView { get; set; }
        public AuthViewModel AuthView { get; set; }
        public ChatCreationViewModel ChatCreationView { get; set; }

        public void HideAll()
        {
            Visibility = false;
            AuthView.Visibility = false;
            ChatCreationView.Visibility = false;
            ChatCreationView.AddUserView.Visibility = false;
            ResoultView.Hide();
        }

        public OverlayGrid(/*MainWindow window*/)
        {
            AuthView = new AuthViewModel();
            ResoultView = new ResoultViewModel();
            ChatCreationView = new ChatCreationViewModel();

            ResoultView.AddBind(AuthView);

            AuthView.Visibility = true;

            AuthView.Success += model =>
            {
                HideAll();
            };
        }
    }
}
