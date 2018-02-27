using System;

namespace QueueLib
{
    /// <summary>
    /// Запрос на подключение к очередям
    /// </summary>
    [Serializable]
    public class MessageChannelRequest : MessageBase
    {
        /// <summary>
        /// Запрашиваемые очереди
        /// </summary>
        public string[] Channels { get; set; }
    }
}
