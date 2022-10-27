using Microsoft.EntityFrameworkCore;
using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerClasses
{
    public class RequestHandler : RequestHandlerBase
    {
        public RequestHandler(RequestListenerBase listenerBase) : base(listenerBase) {}

        public override void OnRegistration(UserCreationModel model)
        {
            try
            {
                using (var db = new ServerDbContext())
                {
                    var user = db.Users.Count() > 0 ? db.Users.Where((u) => u.Login == model.Login).FirstOrDefault() : null;
                    if (user == null)
                    {
                        var newuser = new User()
                        {
                            Login = model.Login,
                            PasswordMD5 = model.PasswordMD5,
                            Name = model.Name,
                            SearchName = model.Name.ToLower() //милиця(костыль) для правильного пошуку користувача
                        };
                        db.Users.Add(newuser);
                        db.SaveChanges();
                        Client.User = newuser;
                    }
                    else
                    {
                        throw new OperationFailureExeption($"Login '{model.Login}' is alrady exist, request rejected");
                    }
                }
                Respondent.ResponseSuccess(BusType.Registration, "You registered successfuly");
                Client.UserOnline();
            }
            catch (OperationFailureExeption ex)
            {
                Respondent.ResponseFailure(BusType.Registration, ex.Message);
            }
        }
        public override void OnAuth(AuthModel model)
        {
            try
            {
                using (var db = new ServerDbContext())
                {
                    var user = db.Users.Count() > 0 ? db.Users.Where((u) => u.Login == model.Login).FirstOrDefault() : null;
                    if (user != null && user.PasswordMD5 == model.PasswordMD5)
                    {
                        Client.User = user;

                    }
                    else
                    {
                        throw new OperationFailureExeption($"Incorrect login or password (а конкретно {(user == null ? "логін" : "пароль")})");
                    }
                }
                Respondent.ResponseSuccess(BusType.Auth, "You authorized successfuly");
                Client.UserOnline();
            }
            catch (OperationFailureExeption ex)
            {
                Respondent.ResponseFailure(BusType.Auth, ex.Message);
            }
        }

        public override void OnChangeChat(ChatChangeModel model)
        {
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

                    model.Users.Add(new IdModel(Client.User.Id));

                    foreach (var user in chat.Users)
                    {
                        if (model.Users.Find(u => u.Id == user.Id) != null)
                        {
                            notChangedUsers.Add(user);
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
                            db.UserChatRelatives.Add(new UserChatRelative()
                            {
                                User = user,
                                Chat = chat,
                                Unreaded = 0
                            });
                        }
                    }
                    db.SaveChanges();
                    Notifyer.ChatChanged(chat, addedUsers, removedUsers, notChangedUsers);
                }
            }
        }

        public override void OnCreateChat(ChatCreationModel model)
        {
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
                model.Users.Add(new IdModel(Client.User.Id));
                db.SaveChanges();

                foreach (var UserIdModel in model.Users)
                {
                    var user = db.Users.Find(UserIdModel.Id);
                    if (user != null)
                    {
                        chat.Users.Add(user);
                        db.UserChatRelatives.Add(new UserChatRelative()
                        {
                            User = user,
                            Chat = chat,
                            Unreaded = 0
                        });
                    }
                }
                db.SaveChanges();

                Notifyer.ChatCreated(db.Chats
                    .Include(o => o.Users)
                    .First(o => o.Id == chat.Id));

            }
            Respondent.ResponseSuccess(BusType.CreateChat, "You successfuly created a new chat");
        }

        public override void OnDeleteChat(IdModel model)
        {
            using (var db = new ServerDbContext())
            {
                var chat = db.Chats
                    .Include(c => c.Users)
                    .Include(c => c.UserChatRelatives)
                    .Include(c => c.Messages)
                    .First(c => c.Id == model.Id);
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
                    Notifyer.ChatDeleted(model.Id, users);
                }
            }
        }

        public override void OnGetAllChats()
        {
            using (var db = new ServerDbContext())
            {
                var chats = from Chat c in db.Chats
                            .Include(c => c.Users)
                            .Include(c => c.Messages)
                            .Include(c => c.UserChatRelatives)
                            where c.Users.Contains(Client.User)
                            select c;
                Respondent.ResponseChats(chats);
            }
        }

        public override void OnGetPageOfMessages(GetMessagesInfoModel model)
        {
            using (var db = new ServerDbContext())
            {
                var messages = db.Messages
                    .Include(m => m.Chat)
                    .Include(m => m.User)
                    .Where(m => m.ChatId == model.ChatId)
                    .OrderByDescending(m => m.SendTime)
                    .Skip(model.From)
                    .Take(ClientObject.PageSize);
                Respondent.ResponseMessagePage(model.From, messages);
            }
        }

        public override void OnMarkReaded(IdModel model)
        {
            using (var db = new ServerDbContext())
            {
                var rel = db.Chats
                    .Include(c => c.UserChatRelatives)
                    .First(c => c.Id == model.Id).UserChatRelatives
                    .First(ucr => ucr.UserId == Client.User.Id);
                rel.Unreaded = 0;
                db.SaveChanges();
            }
        }

        public override void OnReadUnreaded(IdModel model)
        {
            using (var db = new ServerDbContext())
            {
                var rel = db.Chats
                    .Include(c => c.UserChatRelatives)
                    .First(c => c.Id == model.Id).UserChatRelatives
                    .First(ucr => ucr.UserId == Client.User.Id);
                var messages = db.Messages
                    .Include(m => m.Chat)
                    .Include(m => m.User)
                    .Where(m => m.ChatId == model.Id)
                    .OrderByDescending(m => m.SendTime)
                    .Take(rel.Unreaded + ClientObject.PageSize);
                rel.Unreaded = 0;
                db.SaveChanges();

                Respondent.ResponseMessagePage(0, messages);
            }
        }

        public override void OnSearchUsers(SearchModel model)
        {
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

                Client.User = db.Users.Find(Client.User.Id);
                allusers.Remove(Client.User);

                Respondent.ResponseUsers(allusers);
            }
        }

        public override void OnSendMessage(MessageModel model)
        {
            try
            {
                using (var db = new ServerDbContext())
                {
                    if (Client.User.Id == -1) throw new OperationFailureExeption();
                    var message = new Message() { Text = model.Text, UserId = Client.User.Id, ChatId = model.ChatId, SendTime = model.SendTime };
                    db.Messages.Add(message);
                    db.SaveChanges();

                    //var list = new List<ServerDatabase.File>();
                    //foreach (var fileinfo in model.Files)
                    //{
                    //    var file = new ServerDatabase.File() { Message = message, Name = fileinfo.Name, Size = fileinfo.DataSize, Format = fileinfo.Format };
                    //    db.Files.Add(file);
                    //    list.Add(file);
                    //}
                    //db.SaveChanges();
                    //for (int i = 0; i < list.Count; i++)
                    //{
                    //    list[i].ServerPath =
                    //        Network.ReadFile($"Files\\{list[i].Id}_{model.Files[i].Name}.{model.Files[i].Format}",
                    //        model.Files[i]
                    //    );
                    //}
                    //db.SaveChanges();
                    var chat = db.Chats.Include(c => c.Users).First(c => c.Id == model.ChatId);
                    Notifyer.MessageSended(message, chat);
                }
            }
            catch (OperationFailureExeption)
            {
                Respondent.ResponseFailure(BusType.SendMessage, "Unable to send message from unregistered user");
            }
        }
    }
}
