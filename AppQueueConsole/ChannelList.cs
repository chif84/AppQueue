using System.Collections.Generic;

namespace AppQueueConsole
{
    /// <summary>
    /// Менеджер очередей
    /// </summary>
    internal class ChannelList
    {
        #region Ctr

        public ChannelList()
        {
            //генерируем очереди с названиями от "0" до "9"
            Channels = new Dictionary<string, ChannelItem>();
            for (int i = 0; i < 10; i++)
            {
                var channel = new ChannelItem(string.Format("{0}", i));
                Channels.Add(channel.Name, channel);
            }
        }

        #endregion

        /// <summary>
        /// Очереди сообщений
        /// </summary>
        public Dictionary<string, ChannelItem> Channels { get; set; }

        /// <summary>
        /// Получить сообщение из очереди
        /// </summary>
        public string GetItem(string name)
        {
            if (!Channels.ContainsKey(name))
                throw new System.Exception("Channel not found");

            return Channels[name].Get();
        }

        /// <summary>
        /// Положить сообщение в очередь
        /// </summary>
        public void SetItem(string name, string item)
        {
            if (!Channels.ContainsKey(name))
                throw new System.Exception("Channel not found");

            Channels[name].Set(item);
        }
    }
}
