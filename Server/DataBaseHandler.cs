using Microsoft.EntityFrameworkCore;
using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class OperationFailureExeption : Exception
    {
        public BusType? BusType { get; set; }
        public OperationFailureExeption(BusType type, string message) : base(message) { BusType = type; }
        public OperationFailureExeption(string message) : base(message) { }
        public OperationFailureExeption() : base("не вдалося виконати цю операцію") { }
    }
    public class DataBaseHandler
    {
        public static List<int> UsersOnline { get; set; } = new List<int>();
        public static Dictionary<int, Network> NetworkOfId { get; set; } = new Dictionary<int, Network>();
        private User? _user;
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
        }
        public Network network => Client.network;
        public ClientObject Client { get; set; }
        public NotifyUserController Notifyer { get; set; }
        public DataBaseHandler(ClientObject client)
        {
            Client = client;
            Notifyer = new NotifyUserController(client, this);
        }

        public void UserOnline()
        {
            UsersOnline.Add(User.Id);
            NetworkOfId.Add(User.Id, network);
            Notifyer.UserChangeStatus();
        }
        public void UserOffline()
        {
            UsersOnline.Remove(User.Id);
            NetworkOfId.Remove(User.Id);
            using (var db = new ServerDbContext())
            {
                User.LastOnline = DateTime.Now;
                db.SaveChanges();
            }
            Notifyer.UserChangeStatus();
        }
        public List<User> GetRelativeUsers()
        {
            List<User> res = new List<User>();
            using (var db = new ServerDbContext())
            {
                var chats = from Chat c in db.Chats.Include(c => c.Users)
                            where c.Users.Contains(User)
                            select c;
                foreach (var chat in chats)
                {
                    foreach (var user in chat.Users)
                    {
                        if (user.Id != User.Id && !res.Contains(user))
                        {
                            res.Add(user);
                        }
                    }
                }
            }
            return res;
        }

        public void Registration(UserCreationModel model)
        {
            Console.WriteLine($"'{model.Login}' registration");
            using (var db = new ServerDbContext())
            {
                var user = db.Users.Count() > 0 ? db.Users.Where((u) => u.Login == model.Login).FirstOrDefault() : null;
                if (user == null)
                {
                    var newuser = new User() { 
                        Login = model.Login,
                        PasswordMD5 = model.PasswordMD5,
                        Name = model.Name,
                        SearchName = model.Name.ToLower() //милиця(костыль) для правильного пошуку користувача
                    };
                    db.Users.Add(newuser);
                    db.SaveChanges();
                    _user = newuser;
                }
                else
                {
                    throw new OperationFailureExeption($"Login '{model.Login}' is alrady exist, request rejected");
                }
            }
            network.WriteObject(new ResoultModel(BusType.Registration, true, "You registered successfuly"));
            Console.WriteLine($"{User.Login}({User.Id}) registration successfuly");
            UserOnline();
        }
        public void Auth(AuthModel model)
        {
            Console.WriteLine($"'{model.Login}' authorization");
            using (var db = new ServerDbContext())
            {
                var user = db.Users.Count() > 0 ? db.Users.Where((u) => u.Login == model.Login).FirstOrDefault() : null;
                if (user != null && user.PasswordMD5 == model.PasswordMD5)
                {
                    if (!UsersOnline.Contains(user.Id))
                        _user = user;
                    else throw new OperationFailureExeption($"У ваш аккаунт вже виконан вхід з іншого вікна");

                }
                else
                {
                    throw new OperationFailureExeption($"Incorrect login or password (а конкретно {(user == null ? "логін" : "пароль")})");
                }
            }
            network.WriteObject(new ResoultModel(BusType.Auth, true, "You authorized successfuly"));
            Console.WriteLine($"{User.Login}({User.Id}) authorization successfuly");
            UserOnline();
        }

        public void SendMessage(MessageModel model)
        {
            Console.WriteLine($"{User.Login}({User.Id}) request send message to chat ({model.ChatId})");
            try
            {
                using (var db = new ServerDbContext())
                {
                    if (User.Id == -1) throw new OperationFailureExeption();
                    var message = new Message() { Text = model.Text, UserId = User.Id, ChatId = model.ChatId, SendTime = model.SendTime };
                    db.Messages.Add(message);
                    db.SaveChanges();

                    var list = new List<ServerDatabase.File>();
                    foreach (var fileinfo in model.Files)
                    {
                        var file = new ServerDatabase.File() { Message = message, Name = fileinfo.Name, Size = fileinfo.DataSize, Format = fileinfo.Format };
                        db.Files.Add(file);
                        list.Add(file);
                    }
                    db.SaveChanges();
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].ServerPath =
                            network.ReadFile($"Files\\{list[i].Id}_{model.Files[i].Name}.{model.Files[i].Format}",
                            model.Files[i]
                        );
                    }
                    db.SaveChanges();
                    model.Id = message.Id;
                    //network.WriteObject(model);
                    var chat = db.Chats.Include(c => c.Users).Where(c => c.Id == model.ChatId).First();
                    Notifyer.MessageSended(model, chat);
                }
            }
            catch (OperationFailureExeption)
            {
                throw new OperationFailureExeption("Unable to send message from unregistered user");
            }
        }
        public static void AddUnreaded(int chatId, int userId)
        {
            using (var db = new ServerDbContext())
            {
                var rel = (from UserChatRelative ucr in (from Chat c in db.Chats.Include(c => c.UserChatRelatives)
                                                         where c.Id == chatId
                                                         select c).First().UserChatRelatives
                           where ucr.UserId == userId
                           select ucr).First();
                rel.Unreaded++;
                db.SaveChanges();
            }
        }

        public void GetAllChats()
        {
            Console.WriteLine($"{User.Login}({User.Id}) Request for get all chats with him");
            AllChatsModel res;
            using (var db = new ServerDbContext())
            {
                var chats = from Chat c in db.Chats.Include(c => c.Users).Include(c => c.Messages).Include(c => c.UserChatRelatives)
                            where c.Users.Contains(User)
                            select c;
                res = new AllChatsModel();
                res.User = new UserStatusModel()
                {
                    Id = User.Id,
                    Login = User.Login,
                    Name = User.Name,
                    IsOnline = true,
                    LastOnline = null
                };
                res.Chats = new List<ChatModel>();
                foreach (var chat in chats)
                {

                    var userStatusModels = new List<UserStatusModel>();
                    if (chat.Users != null)
                    {
                        foreach (var user in chat.Users)
                        {
                            userStatusModels.Add(new UserStatusModel(user, IsUserOnline(user)));
                        }
                    }

                    MessageModel? messageModel = null;
                    try
                    {
                        var message = chat.Messages.OrderByDescending(c => c.SendTime).First();
                        if (message != null)
                        {
                            User user = db.Users.Find(message.UserId);
                            if (user != null)
                                messageModel = new MessageModel(message, IsUserOnline(user)); // повідомлення, частина якого буде відображатися під чатом, як останне
                        }
                    }
                    catch (Exception) { }

                    res.Chats.Add(new ChatModel()
                    {
                        Id = chat.Id,
                        Title = chat.Title ?? GenerateChatName(chat),
                        IsTrueTitle = chat.Title != null,
                        Users = userStatusModels,
                        LastMessage = messageModel,
                        CreationDate = chat.CreationDate,
                        DateOfChange = chat.DateOfChange,
                        Unreaded = chat.UserChatRelatives.Find(ucr => ucr.User.Id == User.Id).Unreaded
                    }); ;
                }
                network.WriteObject(res);
                Console.WriteLine($"Returned {res.Chats.Count} chats with {res.User.Login}({res.User.Id})");
            }
        }

        public string GenerateChatName(Chat chat)
        {
            var users = from User user in chat.Users
                        where user.Id != User.Id
                        select user;
            var names = new List<string>();
            foreach (var user in users)
            {
                names.Add(user.Name);
            }
            return string.Join(", ", names);
        }
        public string GenerateChatName(ChatModel chat, UserStatusModel userexclusive)
        {
            var users = from UserStatusModel user in chat.Users
                        where user.Id != userexclusive.Id
                        select user;
            var names = new List<string>();
            foreach (var user in users)
            {
                names.Add(user.Name);
            }
            return string.Join(", ", names);
        }
        public static bool IsUserOnline(User user)
        {
            return UsersOnline.Contains(user.Id);
        }

        public void CreateChat(ChatCreationModel model)
        {
            Console.WriteLine($"{User.Login}({User.Id}) Request for create chat");
            using (var db = new ServerDbContext())
            {
                var chat = new Chat();
                chat.Title = model.Title == "" ? null : model.Title;
                chat.CreationDate = DateTime.Now;
                chat.DateOfChange = null;
                chat.Messages = new List<Message>();
                chat.Users = new List<User>();

                db.Chats.Add(chat);
                db.SaveChanges();
                model.Users.Add(new IdModel(User.Id));

                var userStatusModels = new List<UserStatusModel>();
                foreach (var UserIdModel in model.Users)
                {
                    var user = db.Users.Find(UserIdModel.Id);
                    if (user != null)
                    {
                        chat.Users.Add(user);
                        userStatusModels.Add(new UserStatusModel(user, IsUserOnline(user)));
                        db.UserChatRelatives.Add(new UserChatRelative()
                        {
                            User = user,
                            Chat = chat,
                            Unreaded = 0
                        });
                    }
                }
                db.SaveChanges();

                Notifyer.ChatCreated(new ChatModel()
                {
                    Id = chat.Id,
                    CreationDate = chat.CreationDate,
                    Title = chat.Title ?? GenerateChatName(chat),
                    IsTrueTitle = chat.Title != null,
                    Users = userStatusModels,
                    Unreaded = 0,
                    LastMessage = null
                });
                Console.WriteLine($"{User.Login}({User.Id})'s Chat created for {model.Users.Count} users");
            }
            network.WriteObject(new ResoultModel(BusType.CreateChat, true, "You successfuly created a new chat"));
        }

        public void SearchUsers(SearchModel model)
        {
            Console.WriteLine($"{User.Login}({User.Id}) Request for serch users with searchString '{model.SearchString}'");
            var allusers = new List<User>();
            using (var db = new ServerDbContext())
            {
                int id;
                bool IsIdParsed = int.TryParse(model.SearchString, out id);
                if (IsIdParsed)
                {
                    var IdentityId = db.Users.Find(id);
                    if (IdentityId != null && !allusers.Contains(IdentityId))
                        allusers.Add(IdentityId);

                }
                var IdentityLogin = (from User u in db.Users
                                     where u.Login.ToLower() == model.SearchString.ToLower() && !allusers.Contains(u)
                                     select u).FirstOrDefault();
                if (IdentityLogin != null)
                    allusers.Add(IdentityLogin);
                var IdentityName = (from User u in db.Users
                                    where u.Name.ToLower() == model.SearchString.ToLower() && !allusers.Contains(u)
                                    select u).FirstOrDefault();
                if (IdentityName != null)
                    allusers.Add(IdentityName);

                if (IsIdParsed)
                {
                    var SimilarId = from User u in db.Users
                                    where u.Id.ToString().Contains(model.SearchString) && !allusers.Contains(u)
                                    select u;
                    allusers.AddRange(SimilarId);
                }
                var SimilarLogin = from User u in db.Users
                                   where u.Login.ToLower().Contains(model.SearchString.ToLower()) && !allusers.Contains(u)
                                   select u;
                allusers.AddRange(SimilarLogin);
                var SimilarUsername = from User u in db.Users
                                      where u.SearchName.Contains(model.SearchString.ToLower()) && !allusers.Contains(u)
                                      select u;
                allusers.AddRange(SimilarUsername);

                _user = db.Users.Find(User.Id);
                allusers.Remove(_user);

                var res = new AllUsersModel();
                foreach (var user in allusers)
                {
                    res.Users.Add(new UserStatusModel(user, IsUserOnline(user)));
                }
                network.WriteObject(res);
                Console.WriteLine($"For {User.Login}({User.Id})'s search request '{model.SearchString}' Founded {allusers.Count} users");
            }
        }
        public const int PageSize = 20;
        public void GetPageOfMessages(GetMessagesInfoModel model)
        {
            Console.WriteLine($"{User.Login}({User.Id}) Request to get messages in range {model.From}-{model.From + PageSize} from chat ({model.ChatId})");
            using (var db = new ServerDbContext())
            {
                var page = new MessagesPageModel()
                {
                    From = model.From
                };
                var messages = db.Messages
                    .Include(m => m.Chat)
                    .Include(m => m.User)
                    .Where(m => m.ChatId == model.ChatId)
                    .OrderByDescending(m => m.SendTime)
                    .Skip(model.From)
                    .Take(PageSize);
                var count = messages.Count();
                if (count < PageSize) page.IsEnd = true;
                page.To = model.From + count;
                foreach (var message in messages)
                {
                    page.Messages.Add(new MessageModel(message, IsUserOnline(message.User)));
                }
                network.WriteObject(page);
                Console.WriteLine($"{User.Login}({User.Id}) received {page.Messages.Count} messages");
            }
        }

        public void ReadUnreaded(IdModel idModel)
        {
            using (var db = new ServerDbContext())
            {
                var rel = db.Chats
                    .Include(c => c.UserChatRelatives)
                    .First(c => c.Id == idModel.Id).UserChatRelatives
                    .First(ucr => ucr.UserId == User.Id);
                Console.WriteLine($"{User.Login}({User.Id}) Request for get unreaded messages in the amount of {rel.Unreaded} from chat ({idModel.Id})");
                var page = new MessagesPageModel()
                {
                    From = 0,
                    To = rel.Unreaded + PageSize
                };
                var messages = db.Messages
                    .Include(m => m.Chat)
                    .Include(m => m.User)
                    .Where(m => m.ChatId == idModel.Id)
                    .OrderByDescending(m => m.SendTime)
                    .Take(page.To);
                var count = messages.Count();
                if (count < PageSize) page.IsEnd = true;
                page.To = count;
                foreach (var message in messages)
                {
                    page.Messages.Add(new MessageModel(message, IsUserOnline(message.User)));
                }
                rel.Unreaded = 0;
                db.SaveChanges();
                network.WriteObject(page);
            }
        }
        public void MarkReaded(IdModel idModel)
        {
            using (var db = new ServerDbContext())
            {
                var rel = db.Chats
                    .Include(c => c.UserChatRelatives)
                    .First(c => c.Id == idModel.Id).UserChatRelatives
                    .First(ucr => ucr.UserId == User.Id);
                if (rel.Unreaded > 1) Console.WriteLine($"{User.Login}({User.Id}) mark readed messages in the amount of {rel.Unreaded} from chat ({idModel.Id})");
                rel.Unreaded = 0;
                db.SaveChanges();
            }
        }

        public void ChangeChat(ChatChangeModel model)
        {
            Console.WriteLine($"{User.Login}({User.Id}) Request for change chat ({model.Id})");
            using (var db = new ServerDbContext())
            {
                var chat = db.Chats
                    .Include(c => c.Users)
                    .Include(c => c.Messages)
                    .Include(c => c.UserChatRelatives)
                    .First(c => c.Id == model.Id);
                if (chat != null)
                {
                    chat.Title = model.Title;
                    chat.DateOfChange = DateTime.Now;
                    var addedUsers = new List<User>();
                    var removedUsers = new List<User>();
                    var notChangedUsers = new List<User>();

                    model.Users.Add(new IdModel(User.Id));

                    var userStatusModels = new List<UserStatusModel>();
                    foreach (var user in chat.Users)
                    {
                        if (model.Users.Find(u => u.Id == user.Id) != null)
                        {
                            notChangedUsers.Add(user);
                            userStatusModels.Add(new UserStatusModel(user, IsUserOnline(user)));
                        }
                        else
                        {
                            removedUsers.Add(user);
                        }
                    }
                    foreach (var user in removedUsers)
                    {
                        chat.Users.Remove(user);
                        var ucr = db.UserChatRelatives.Find(user.Id, chat.Id);
                        if (ucr != null) db.UserChatRelatives.Remove(ucr);
                    }
                    foreach (var UserIdModel in model.Users)
                    {
                        var user = db.Users.Find(UserIdModel.Id);
                        if (user != null && !chat.Users.Contains(user))
                        {
                            addedUsers.Add(user);
                            chat.Users.Add(user);
                            userStatusModels.Add(new UserStatusModel(user, IsUserOnline(user)));
                            db.UserChatRelatives.Add(new UserChatRelative()
                            {
                                User = user,
                                Chat = chat,
                                Unreaded = 0
                            });
                        }
                    }
                    MessageModel? messageModel = null;
                    try
                    {
                        var message = chat.Messages.OrderByDescending(c => c.SendTime).FirstOrDefault();
                        if (message != null)
                        {
                            User? user = db.Users.Find(message.UserId);
                            if (user != null)
                                messageModel = new MessageModel(message, IsUserOnline(user)); // повідомлення, частина якого буде відображатися під чатом, як останне
                        }
                    }
                    catch (Exception) { }
                    db.SaveChanges();

                    Notifyer.ChatChanged(new ChatModel()
                    {
                        Id = chat.Id,
                        CreationDate = chat.CreationDate,
                        DateOfChange = chat.DateOfChange,
                        Title = chat.Title ?? GenerateChatName(chat),
                        IsTrueTitle = chat.Title != null,
                        Users = userStatusModels,
                        Unreaded = 0,
                        LastMessage = messageModel
                    }, addedUsers, removedUsers, notChangedUsers);
                }
            }
        }
        public void DeleteChat(IdModel idModel)
        {
            Console.WriteLine($"{User.Login}({User.Id}) Request for delete chat ({idModel.Id})");
            using (var db = new ServerDbContext())
            {
                var chat = db.Chats
                    .Include(c => c.Users)
                    .Include(c => c.UserChatRelatives)
                    .Include(c => c.Messages)
                    .First(c => c.Id == idModel.Id);
                if (chat != null)
                {
                    var users = new List<User>(chat.Users);
                    var relatives = new List<UserChatRelative>(chat.UserChatRelatives);
                    var messages = new List<Message>(chat.Messages);
                    chat.Users.Clear();
                    chat.UserChatRelatives.Clear();
                    chat.Messages.Clear();
                    foreach (var ucr in relatives)
                    {
                        db.UserChatRelatives.Remove(ucr);
                    }
                    foreach (var user in users)
                    {
                        user.Chats.Remove(user.Chats.Find(c => c.Id == chat.Id));
                    }
                    foreach (var msg in messages)
                    {
                        db.Messages.Remove(msg);
                    }
                    db.Chats.Remove(chat);
                    db.SaveChanges();
                    Notifyer.ChatDeleted(idModel, users);
                }
            }
        }
    }
}
