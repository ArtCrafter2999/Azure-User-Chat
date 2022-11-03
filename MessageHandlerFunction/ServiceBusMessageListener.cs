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
        public void Invoke(RequestWrap wrap)
        {
            if (wrap.Type != RequestType.Registration && wrap.Type != RequestType.Auth) Client.SetUser(int.Parse(wrap.ID));
            Invoke(wrap.Type, wrap.RawObject);
        }
    }
}
