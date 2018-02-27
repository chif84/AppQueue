using System;

namespace QueueLib
{
    /// <summary>
    /// Сообщение с элементом очереди
    /// </summary>
    [Serializable]
    public class MessageChannelMessage : MessageBase
    {
        /// <summary>
        /// Название очереди
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Содержимое элемента очереди
        /// </summary>
        public string Text { get; set; }
    }
}
