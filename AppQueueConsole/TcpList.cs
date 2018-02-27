using QueueLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AppQueueConsole
{
    /// <summary>
    /// Менеджер управления подключениями по TCP для отправки сообщений
    /// </summary>
    internal class TcpList
    {
        #region Ctr

        public TcpList(ChannelList channelList)
        {
            this.channelList = channelList;
            tcpItemList = new List<TcpItem>();
        }

        #endregion

        #region Private

        /// <summary>
        /// Очереди
        /// </summary>
        private ChannelList channelList;

        /// <summary>
        /// Листенер прослушки Tcp
        /// </summary>
        private TcpListener tcpListener;

        private object internalLock = new object();

        /// <summary>
        /// Подключенные клиенты
        /// </summary>
        private List<TcpItem> tcpItemList;

        #endregion

        /// <summary>
        /// Добавление клиента
        /// </summary>
        public bool AddItem(TcpItem item, MessageChannelRequest request)
        {
            lock (internalLock)
            {
                if (item == null || request == null)
                    return false;

                //отказ если очередь уже слушается
                //if (tcpItemList.Any(i => i.Channels != null && i.Channels.Any(c => request.Channels.Any(rc => rc == c.Name))))
                var connectedItem = tcpItemList.FirstOrDefault(i => i.Channels != null && i.Channels.Any(c => request.Channels.Any(rc => rc == c.Name)));
                if (connectedItem != null)
                {
                    if (connectedItem.IsActived())
                        return false;
                    else
                        RemoveItem(connectedItem.Uid);
                }


                //привязка очереди к клиент если доступна
                item.Channels = channelList.Channels.Values.Where(c => request.Channels.Any(rc => rc == c.Name)).ToList();
                item.Channels.ForEach(c => c.Item = item);

                //добавление клиента в список прослушивающих
                tcpItemList.Add(item);

                return true;
            }
        }

        /// <summary>
        /// Удаление клиента
        /// </summary>
        /// <param name="uid"></param>
        public void RemoveItem(Guid uid)
        {
            lock (internalLock)
            {
                var item = tcpItemList.FirstOrDefault(c => c.Uid == uid);
                if (item != null)
                {
                    item.Channels.ForEach(c => c.Item = null);
                    tcpItemList.Remove(item);
                    Console.WriteLine("disconnect {0}", item.Uid);
                }
            }
        }

        /// <summary>
        /// Начало прослушивания для отправки сообщений
        /// </summary>
        public void ListenList()
        {
            try
            {
                tcpListener = new TcpListener(HostConstants.HostAddress, HostConstants.Port);
                tcpListener.Start();
                Console.WriteLine("await connection");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    //новые клиенты начинаем слушать в отдельном потоке
                    var clientObject = new TcpItem(tcpClient, this);
                    var clientThread = new Thread(new ThreadStart(clientObject.ListenItem));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                DisconnectList();
            }
        }

        /// <summary>
        /// Закрытие всех подключений
        /// </summary>
        public void DisconnectList()
        {
            tcpListener.Stop();

            for (int i = 0; i < tcpItemList.Count; i++)
            {
                tcpItemList[i].Close();
            }
            Environment.Exit(0);
        }
    }


}
