using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
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
            Logger?.LogInformation("ServiceBusMessageListener Invoke with id: " + wrap.ID);
            try
            {
                if (wrap.Type != RequestType.Registration && wrap.Type != RequestType.Auth) Client.SetUser(int.Parse(wrap.ID));
                Invoke(wrap.Type, wrap.RawObject);
            }
            catch (Exception ex)
            {
                Logger?.LogError("ServiceBusMessageListener Exeption: {0}", ex.Message);
            }
            
        }
    }
}
