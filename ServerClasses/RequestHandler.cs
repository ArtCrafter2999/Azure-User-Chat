using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        public async override Task OnRegistration(UserCreationModel model)
        {
            Logger?.LogInformation("Handler OnRegistration");
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
                        Logger?.LogInformation("Handler OnRegistration: successfuly added new user");
                        Client.User = newuser;
                    }
                    else
                    {
                        throw new OperationFailureException($"Login '{model.Login}' is alrady exist, request rejected");
                    }
                }
                await Respondent.ResponseId(Client.User.Id);
                Client.UserOnline();
            }
            catch (OperationFailureException ex)
            {
                Logger?.LogError("Handler OnRegistration OperationFailureException: {0}         StackTrace: {1}", ex.Message, ex.StackTrace);
                await Respondent.ResponseId(-1);
            }
            catch (Exception ex)
            {
                Logger?.LogError("Handler OnRegistration Exception: {0}         StackTrace: {1}", ex.Message, ex.StackTrace);
                await Respondent.ResponseId(-1);
            }
        }
        public async override Task OnAuth(AuthModel model)
        {
            Logger?.LogInformation("Handler OnAuth");
            try
            {
                using (var db = new ServerDbContext())
                {
                    Logger?.LogInformation("Handler OnAuth Db Connected");
                    var user = db.Users.Count() > 0 ? db.Users.Where((u) => u.Login == model.Login).FirstOrDefault() : null;
                    if (user != null && user.PasswordMD5 == model.PasswordMD5)
                    {
                        Logger?.LogInformation("Handler OnAuth User founded");
                        Client.User = user;
                    }
                    else
                    {
                        throw new OperationFailureException($"Incorrect login or password (а конкретно {(user == null ? "логін" : "пароль")})");
                    }
                }
                Logger?.LogInformation("Handler OnAuth Response id: {0}", Client.User.Id);
                await Respondent.ResponseId(Client.User.Id);
                Logger?.LogInformation("Handler OnAuth UserOnline()");
                Client.UserOnline();
            }
            catch (OperationFailureException ex)
            {
                Logger?.LogError("Handler OnAuth OperationFailureException: " + ex.Message);
                await Respondent.ResponseId(-1);
            }
            catch (Exception ex)
            {
                Logger?.LogError("Handler OnAuth Exception: " + ex.Message);
                await Respondent.ResponseId(-1);
            }
        }

        public async override Task OnChangeChat(ChatChangeModel model)
        {
            Logger?.LogInformation("Handler OnChangeChat");

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
                    await Notifyer.ChatChanged(chat, addedUsers, removedUsers, notChangedUsers);
                }
            }
        }

        public async override Task OnCreateChat(ChatCreationModel model)
        {
            Logger?.LogInformation("Handler OnCreateChat");

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

                await Notifyer.ChatCreated(db.Chats
                    .Include(o => o.Users)
                    .First(o => o.Id == chat.Id));

            }
            await Respondent.ResponseSuccess(RequestType.CreateChat, "You successfuly created a new chat");
        }

        public async override Task OnDeleteChat(IdModel model)
        {
            Logger?.LogInformation("Handler OnDeleteChat");

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
                    await Notifyer.ChatDeleted(model.Id, users);
                }
            }
        }

        public async override Task OnGetAllChats()
        {
            Logger?.LogInformation("Handler OnGetAllChats");
            try
            {
                using (var db = new ServerDbContext())
                {
                    var chats = from Chat c in db.Chats
                                .Include(c => c.Users)
                                .Include(c => c.Messages)
                                .Include(c => c.UserChatRelatives)
                                where c.Users.Contains(Client.User)
                                select c;
                    Logger?.LogInformation("Handler OnGetAllChats: count of chats: {0}", chats.Count());
                    await Respondent.ResponseChats(chats);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError("Handler OnGetAllChats Exception: {0}         StackTrace: {1}", ex.Message, ex.StackTrace);
            }
            
        }

        public async override Task OnGetPageOfMessages(GetMessagesInfoModel model)
        {
            Logger?.LogInformation("Handler OnGetPageOfMessages");

            using (var db = new ServerDbContext())
            {
                var messages = db.Messages
                    .Include(m => m.Chat)
                    .Include(m => m.User)
                    .Where(m => m.ChatId == model.ChatId)
                    .OrderByDescending(m => m.SendTime)
                    .Skip(model.From)
                    .Take(ClientObject.PageSize);
                await Respondent.ResponseMessagePage(model.From, messages);
            }
        }

        public async override Task OnMarkReaded(IdModel model)
        {
            Logger?.LogInformation("Handler OnMarkReaded");

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

        public async override Task OnReadUnreaded(IdModel model)
        {
            Logger?.LogInformation("Handler OnReadUnreaded");

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

                await Respondent.ResponseMessagePage(0, messages);
            }
        }

        public async override Task OnSearchUsers(SearchModel model)
        {
            Logger?.LogInformation("Handler OnSearchUsers");

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

                allusers.Remove(Client.User);

                await Respondent.ResponseUsers(allusers);
            }
        }

        public async override Task OnSendMessage(MessageModel model)
        {
            Logger?.LogInformation("Handler OnSendMessage");

            try
            {
                using (var db = new ServerDbContext())
                {
                    if (Client.User.Id == -1) throw new OperationFailureException();
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
                    await Notifyer.MessageSended(message, chat);
                }
            }
            catch (OperationFailureException)
            {
                await Respondent.ResponseFailure(RequestType.SendMessage, "Unable to send message from unregistered user");
            }
        }

        public override async Task OnLogOut()
        {
            await Task.Run(() => Client.UserOffline());
        }
    }
}
