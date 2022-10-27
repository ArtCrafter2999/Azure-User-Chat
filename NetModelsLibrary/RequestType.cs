using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary
{
    public enum BusType
    {
        /// <summary>
        /// Registration request which contain login username and password in MD5
        /// After this server expects that sequence of requests:
        /// Expects UserCreationModel returns ResoultModel
        /// </summary>
        Registration,
        /// <summary>
        /// Request for send message from the user selected chat
        /// After this server expects MessageModel
        /// </summary>
        SendMessage,
        /// <summary>
        /// Authorization request which contain login and password in MD5
        /// After this server expects that sequence of requests:
        /// Expects AuthModel returns ResoultModel
        /// </summary>
        Auth,
        /// <summary>
        /// Request to get all chats which the user is a member
        /// After this server not expects anything, instred it returns AllChatsModel for current user
        /// </summary>
        GetAllChats,
        /// <summary>
        /// Request to create chat
        /// After this server expects that sequence of requests:
        /// Expects ChatCreationModel returns ResoultModel 
        /// </summary>
        CreateChat,
        /// <summary>
        /// Searching for a new user to add to a new chat
        /// After this server expects that sequence of requests:
        /// Expects SearchModel returns AllUsersModel 
        /// </summary>
        SearchUsers,
        /// <summary>
        /// Page by page to get messages
        /// After this server expects that sequence of requests:
        /// Expects GetMessagesInfoModel returns MessagesPageModel
        /// </summary>
        GetPageOfMessages,
        /// <summary>
        /// Get all unreaded messages and mark them readed
        /// After this server expects that sequence of requests:
        /// Expects IdModel witch is chat id and returns MessagesPageModel
        /// </summary>
        ReadUnreaded,
        /// <summary>
        /// Mark all unreaded messages as readed
        /// After this server expects that sequence of requests:
        /// Expects IdModel witch is chat id. Returns nothing
        /// </summary>
        MarkReaded,
        /// <summary>
        /// Request to change chat
        /// After this server expects that sequence of requests:
        /// Expects ChatChangeModel. Returns nothing  
        /// </summary>
        ChangeChat,
        /// <summary>
        /// Request to delete chat
        /// After this server expects that sequence of requests:
        /// Expects IdModel witch is chat id. Returns nothing  
        /// </summary>
        DeleteChat,




        /// <summary>
        /// Notify about chat with the user has been created.
        /// Send the created ChatModel
        /// </summary>
        ChatCreated,
        /// <summary>
        /// Notify that someone has sent a message to one of the chats
        /// Send the sended MessageModel
        /// </summary>
        MessageSended,
        /// <summary>
        /// Notify that user chatget his status (online or offline)
        /// Send the sended UserStatusModel
        /// </summary>
        UserChangeStatus,
        /// <summary>
        /// Notify about chat with the user has been changed.
        /// Notify sender too
        /// Send the changed ChatModel
        /// </summary>
        ChatChanged,
        /// <summary>
        /// Notify about chat with the user has been deleted or user has been removed from the chat.
        /// Notify sender too
        /// Send the IdModel of deleted chat
        /// </summary>
        ChatDeleted,
    }
}
