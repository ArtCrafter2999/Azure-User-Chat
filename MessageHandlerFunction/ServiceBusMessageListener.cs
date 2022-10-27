using Azure.Messaging.ServiceBus;
using NetModelsLibrary;
using ServerClasses;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MessageHandlerFunction
{
    internal class ServiceBusMessageListener : RequestListenerBase
    {
        public void Invoke(ServiceBusReceivedMessage message)
        {
            Endpoint = new ReplyEndpoint(message.SessionId);
            Invoke(message.Body.ToString());
        }
    }
}
