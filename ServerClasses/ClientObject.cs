using Microsoft.EntityFrameworkCore;
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
        private static Dictionary<int, ClientObject> UsersOnline { get; set; } = new Dictionary<int, ClientObject>();
        public const int PageSize = 20;

        private User? _user;

        public event Action<User> OnDisconected;

        public User User
        {
            get
            {
                if (_user != null) return _user;
                else return new User()
                {
                    Id = -1,
                    Login = "unknown",
                    Name = "unknown",
                    PasswordMD5 = "unknown"
                };
            }
            set { _user = value; }
        }

        public override void Disconect()
        {
            UserOffline();
            OnDisconected?.Invoke(User);
        }

        public override void UserOnline()
        {
            //try
            //{
            //    UsersOnline.Add(User.Id, this);
            //    Notifyer.UserChangeStatus();
            //}
            //catch { }
        }
        public override void UserOffline()
        {
            //try
            //{
            //    UsersOnline.Remove(User.Id);
            //    using (var db = new ServerDbContext())
            //    {
            //        User.LastOnline = DateTime.Now;
            //        db.SaveChanges();
            //    }
            //    Notifyer.UserChangeStatus();
            //}
            //catch{}
        }

        public override ServerEndpoint GetOnlineUserEndpoint(int userId)
        {
            return new ServerEndpoint(userId.ToString());
        }

        public override bool IsUserOnline(int userId)
        {
            return true /*UsersOnline.ContainsKey(userId)*/;
        }

        public override void SetUser(int userId)
        {
            using (ServerDbContext db = new ServerDbContext())
            {
                _user = db.Users.Find(userId);
            }
        }
    }
}
