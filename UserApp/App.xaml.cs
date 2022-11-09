using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace UserApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void Application_Exit(object sender, ExitEventArgs e)
        {
            await Connection.Endpoint.SendObject(new RequestWrap(RequestType.LogOut, Connection.Endpoint.sessionId));
            if (Connection.IsConnected)
            {
                Connection.Disconect();
            }
        }
    }
}
