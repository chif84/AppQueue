using System;

namespace QueueLib
{
    /// <summary>
    /// Ответ на запрос подключения к очереди
    /// </summary>
    [Serializable]
    public class MessageChannelResponce : MessageBase
    {
        /// <summary>
        /// Uid подключения полученный на сервере
        /// </summary>
        public Guid Uid { get; set; }

        /// <summary>
        /// Разрешено ли подключение по запрошенным очередям
        /// </summary>
        public bool Approve { get; set; }
    }

}
