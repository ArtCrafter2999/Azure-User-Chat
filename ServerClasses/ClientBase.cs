using NetModelsLibrary;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public abstract class ClientBase : ClientModelBase
    {

        public event Action<User> OnDisconected;

        public abstract ServerEndpoint GetOnlineUserEndpoint(int userId);
        public abstract bool IsUserOnline(int userId);
        public abstract void Disconect();
        public abstract void UserOnline();
        public abstract void UserOffline();

        public abstract void SetUser(int userId);

        public User User { get; set; }
    }
}
