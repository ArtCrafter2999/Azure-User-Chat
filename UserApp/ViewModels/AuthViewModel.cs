using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Commands;
using NetModelsLibrary.Models;
using NetModelsLibrary;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace UserApp.ViewModels
{
    public class AuthViewModel : EndpointResoulter, INotifyPropertyChanged
    {
        public string? Login { get; set; }
        public string? Username { get; set; }

        public bool Visibility { get => _visibility; set { _visibility = value; OnPropertyChanged(nameof(Visibility)); } }
        private bool _visibility = false;

        public void Connect()
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                Connection.EndpointCancelation = new CancellationTokenSource();
                Connection.Endpoint = new ClientEndpoint()
                {Token = Connection.EndpointCancelation.Token };
                Connection.IsConnected = true;
            }
            catch (Exception)
            {
            }
        }

        public ICommand Authorize => new RelayCommand(async o =>
        {
            Connect();
            if (Connection.IsConnected)
            {
                Connection.Endpoint.SendRequest(new AuthModel() { Type = BusType.Auth, Login = Login, PasswordMD5 = CreateMD5(((PasswordBox)o).Password) });
                Invoke(await Connection.Endpoint.ReceiveReply<ResoultModel>());
            }
            else
            {
                Invoke(new ResoultModel(false, "Немає з'єднання з сервером"));
            }
        }, o => o != null && Login != null);
        public ICommand Register => new RelayCommand(async o =>
        {
            Connect();
            if (Connection.IsConnected)
            {
                Connection.Endpoint.SendRequest(new UserCreationModel() { Type = BusType.Registration, Name = Username, Login = Login, PasswordMD5 = CreateMD5(((PasswordBox)o).Password) });
                try
                {
                    ResoultModel? resoult = await Connection.Endpoint.ReceiveReply<ResoultModel>();
                    Invoke(resoult);
                }
                catch (Exception ex)
                {
                    Invoke(new ResoultModel(false, ex.Message));
                    throw;
                }
            }
            else
            {
                Invoke(new ResoultModel(false, "Немає з'єднання з сервером"));
            }
        }, o => o != null && Login != null && Username != null);

        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
