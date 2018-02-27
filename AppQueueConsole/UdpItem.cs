using QueueLib;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace AppQueueConsole
{
    /// <summary>
    /// Управление приемом сообщений по Udp
    /// </summary>
    internal class UdpItem
    {

        #region Ctr

        public UdpItem(ChannelList channelList)
        {
            this.channelList = channelList;
        }

        #endregion

        #region Private

        /// <summary>
        /// Очереди
        /// </summary>
        private ChannelList channelList;

        #endregion

        /// <summary>
        /// Начало прослушивания для приема сообщений
        /// </summary>
        public void Listen()
        {
            //
            using (var client = new UdpClient(HostConstants.Port))
            {
                IPEndPoint ip = null;

                while (true)
                {
                    //ждем сообщение
                    var bytes = client.Receive(ref ip);

                    //TODO это в отдельный поток
                    {
                        //полученное сообщение кладем в очередь
                        var message = MessageBase.FromBytes(bytes) as MessageChannelMessage;
                        if (message != null)
                        {
                            if (channelList.Channels.Any(c => c.Key == message.Name))
                            {
                                channelList.Channels.FirstOrDefault(c => c.Key == message.Name).Value.Set(message.Text);
                                Console.WriteLine("message added for channel {0}: {1}", message.Name, message.Text);
                            }
                            else
                                Console.WriteLine("message skipped for channel {0}", message.Name);
                        }
                    }
                }

            }
        }
    }

}
