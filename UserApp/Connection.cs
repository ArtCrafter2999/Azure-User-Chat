using Azure.Messaging.ServiceBus;
using NetModelsLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UserApp.Controllers;

namespace UserApp
{
    public static class Connection
    {
        public static ClientEndpoint Endpoint { get; set; }
        public static bool IsConnected { get; set; } = false;
        public static CancellationTokenSource EndpointCancelation;
        public static void Disconect()
        {
            EndpointCancelation.Cancel();
        }
    }
}
