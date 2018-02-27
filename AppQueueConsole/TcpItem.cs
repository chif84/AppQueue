using QueueLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace AppQueueConsole
{
    /// <summary>
    /// Обмен сообшениями с подключенным клиентом
    /// </summary>
    internal class TcpItem
    {

        #region Ctr

        public TcpItem(TcpClient tcpClient, TcpList tcpList)
        {
            Uid = Guid.NewGuid();
            client = tcpClient;

            this.tcpList = tcpList;

        }

        #endregion

        #region Private

        private object internalLock = new object();

        private NetworkStream stream;

        private TcpClient client;

        /// <summary>
        /// Управляющий менеджер
        /// </summary>
        private TcpList tcpList;

        #endregion

        public Guid Uid { get; private set; }

        public AutoResetEvent NewItemEvent = new AutoResetEvent(true);

        /// <summary>
        /// Список очередей из которых клиент получает данные
        /// </summary>
        public List<ChannelItem> Channels { get; set; }

        public bool IsActived()
        {
            return !(((client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected));
        }

        /// <summary>
        /// Начало обмена сообщениями
        /// </summary>
        public void ListenItem()
        {
            try
            {
                stream = client.GetStream();

                //отправляем приветствие
                var helloMessage = new MessageChannelHello();
                var messageData = MessageBase.ToBytes(helloMessage);
                stream.Write(messageData, 0, messageData.Length);
                Console.WriteLine("hello {0}", Uid);

                //получаем запрос на подключение к очередям
                
                var channelRequest = ReceiveMessage() as MessageChannelRequest;

                Console.WriteLine("connect {0}", Uid);

                //если запрашиваемые очереди не заняты другими клиентами то добавляем
                if (channelRequest != null && tcpList.AddItem(this, channelRequest))
                {
                    //возвращаем подтверждение о добавлении
                    var channelResponce = new MessageChannelResponce() { Uid = Uid, Approve = true };
                    var data = MessageBase.ToBytes(channelResponce);
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("connection {0} approved channels {1}", Uid, string.Join(",", channelRequest.Channels));

                    Thread.Sleep(1000);

                    //отправка из очередей
                    try
                    {
                        while (true)
                        {
                            NewItemEvent.Reset();

                            Channels.ForEach(c => {
                                string text = null;
                                do {
                                    text = c.Peek();
                                    if (text != null)
                                    {
                                        SendMessage(c.Name, text);
                                        Thread.Sleep(100);

                                        var messageResponce = ReceiveMessage() as MessageChannelMessageResponce;
                                        if (messageResponce != null)
                                        {
                                            c.Get();
                                            Thread.Sleep(100);
                                        }
                                        else
                                        {
                                            throw new Exception(string.Format("connection {0} incorrect responce", Uid));
                                        }
                                    }
                                    else
                                    {
                                        //закрываем соединение если клиент неожиданно откулючается
                                        if (!IsActived())
                                        {
                                            throw new Exception(string.Format("connection {0} aborted", Uid));
                                        }
                                    }
                                }
                                while (text != null);
                            });

                            NewItemEvent.WaitOne(10000);
                            //Thread.Sleep(1000);
                        }
                    }
                    catch
                    {
                        //TODO сюда добавить адекватный лог
                    }
                }
                else
                {
                    //если запрашиваемые очереди заняты другими клиентами то отказываем
                    var message = new MessageChannelResponce() { Uid = Uid, Approve = false };
                    var data = MessageBase.ToBytes(message);
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("connection {0} disapproved channels {1}", Uid, string.Join(",", channelRequest.Channels));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                //закрытие подключения и исключение из списка подключений
                tcpList.RemoveItem(Uid);
                Close();
            }
        }

        /// <summary>
        /// Прием сообщения
        /// </summary>
        private IMessageBase ReceiveMessage()
        {
            byte[] buffer = new byte[64];
            var bytes = new byte[] { };

            int bytesCnt = 0;
            do
            {
                bytesCnt = stream.Read(buffer, 0, buffer.Length);
                bytes = bytes.Concat(buffer).ToArray();
            }
            while (stream.DataAvailable);

            return MessageBase.FromBytes(bytes);
        }

        /// <summary>
        /// Отправка сообщения
        /// </summary>
        private void SendMessage(string channel, string text)
        {
            lock (internalLock)
            {
                var message = new MessageChannelMessage();
                message.Name = channel;
                message.Text = text;

                var bytes = MessageBase.ToBytes(message);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        //Закрытие потока и самого подключения
        internal void Close()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
        }
    }

}
