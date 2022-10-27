using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetModelsLibrary;
using NetModelsLibrary.Models;
using ServerClasses;

namespace Server
{
    public class ConsoleOutput
    {
        public void Bind(RequestListenerBase events)
        {
            events.OnAuth += OnAuth;
            events.OnChangeChat += OnChangeChat;
            events.OnCreateChat += OnCreateChat;
            events.OnDeleteChat += OnDeleteChat;
            events.OnGetAllChats += OnGetAllChats;
            events.OnGetPageOfMessages += OnGetPageOfMessages;
            events.OnMarkReaded += OnMarkReaded;
            events.OnReadUnreaded += OnReadUnreaded;
            events.OnRegistration += OnRegistration;
            events.OnSearchUsers += OnSearchUsers;
            events.OnSendMessage += OnSendMessage;
        }

        public void OnAuth(AuthModel model)
        {
            Console.WriteLine($"Request to auth for '{model.Login}'");
        }

        public void OnChangeChat(ChatChangeModel model)
        {
            Console.WriteLine($"Request to change {model.Title}({model.Id}) chat");
        }

        public void OnCreateChat(ChatCreationModel model)
        {
            Console.WriteLine($"Request to create chat with name {model.Title}");
        }

        public void OnDeleteChat(IdModel model)
        {
            Console.WriteLine($"Request to delete {model.Id} chat");
        }

        public void OnGetAllChats()
        {
            Console.WriteLine($"Request to get all chats");
        }

        public void OnGetPageOfMessages(GetMessagesInfoModel model)
        {
            Console.WriteLine($"Request to get {model.From}-{ServerClasses.ClientObject.PageSize} messages from {model.ChatId} chat");
        }

        public void OnMarkReaded(IdModel model){}

        public void OnReadUnreaded(IdModel model)
        {
            Console.WriteLine($"Request to get unreaded messages from {model.Id} chat");
        }

        public void OnRegistration(UserCreationModel model)
        {
            Console.WriteLine($"Request to registration for '{model.Login}'");
        }

        public void OnSearchUsers(SearchModel model)
        {
            Console.WriteLine($"Request to search people by '{model.SearchString}' search string");
        }

        public void OnSendMessage(MessageModel model)
        {
            Console.WriteLine($"Request to send message to {model.ChatId} chat");
        }

        public void OnConnected()
        {
            Console.WriteLine($"Unknown user has connected");
        }

        public void OnSuccess(BusType type, string message)
        {
            Console.WriteLine($"{type} resoult: Success; Description: {message}");
        }
        public void OnFailure(BusType type, string message)
        {
            Console.WriteLine($"{type} resoult: Failure; Description: {message}");
        }
        public void OnDisconected(ServerDatabase.User user)
        {
            Console.WriteLine($"User '{user.Login}'({user.Id}) disconnected");
        }
    }
}
