using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetModelsLibrary;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public class ClientObject : ClientBase
    {
        private static HashSet<int> UsersOnline { get; set; } = new HashSet<int>();
        public const int PageSize = 20;

        private int? _userId;

        public event Action<User> OnDisconected;

        public override User User
        {
            get
            {
                if (_userId > 0)
                {
                    User? user;
                    using (ServerDbContext db = new ServerDbContext())
                    {
                        user = db.Users.Find(_userId);
                    }
                    if (user != null) return user;
                }
                return new User()
                {
                    Id = -1,
                    Login = "unknown",
                    Name = "unknown",
                    PasswordMD5 = "unknown"
                };
            }
            set { _userId = value.Id; }
        }

        public override void Disconect()
        {
            UserOffline();
            OnDisconected?.Invoke(User);
        }

        public override void UserOnline()
        {
            if (_userId != null) UsersOnline.Add(_userId.Value);
            Notifyer.UserChangeStatus();
        }
        public override void UserOffline()
        {
            if (_userId != null) UsersOnline.Remove(_userId.Value);
            Notifyer.UserChangeStatus();
        }

        public override ServerEndpoint GetOnlineUserEndpoint(int userId)
        {
            return new ServerEndpoint(userId.ToString());
        }

        public override bool IsUserOnline(int userId)
        {
            return UsersOnline.Contains(userId);
        }

        public override void SetUser(int userId)
        {
            Logger?.LogInformation("ClientObject SetUser with id: " + userId);
            try
            {
                using (ServerDbContext db = new ServerDbContext())
                {
                    var user = db.Users.Find(userId);
                    if (user == null)
                    {
                        throw new Exception($"User with this id '{userId}' not found");
                    }
                    Logger?.LogInformation($"User found: Id: {user.Id}, Login: {user.Login}, Name: {user.Name}");
                    _userId = user.Id;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError("ClientObject SetUser Exception: {0}         StackTrace: {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
