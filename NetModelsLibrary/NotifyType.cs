using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetModelsLibrary
{
    public enum NotifyType
    {
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
