using QueueLib;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace AppGetConsole
{
    /// <summary>
    /// Получения сообщений
    /// </summary>
    internal class ProcessingGet
    {
        static Random rnd = new Random();

        public void Processing()
        {
            //генерация случайных очередей для получения сообщений
            var channel1 = rnd.Next(10);
            var channel2 = rnd.Next(10);
            Console.WriteLine("channel: {0}", channel1);
            Console.WriteLine("channel: {0}", channel2);

            NetworkStream stream = null;

            var client = new TcpClient();
            try
            {
                client.Connect(HostConstants.Host, HostConstants.Port);

                stream = client.GetStream();

                Console.WriteLine("await server hello");

                //получаем приветственное сообщение
                var helloMessage = ReceiveMessage(stream) as MessageChannelHello;

                if (helloMessage == null)
                {
                    Console.WriteLine("unknown server");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("server: hello");

                    //отправляем запрос на подключение к очередям
                    var channelRequest = new MessageChannelRequest();
                    channelRequest.Channels = new string[] { channel1.ToString(), channel2.ToString() };
                    byte[] data = MessageBase.ToBytes(channelRequest);
                    stream.Write(data, 0, data.Length);

                    Thread.Sleep(500);

                    //получаем ответ на запрос подключения
                    var channelResponce = ReceiveMessage(stream) as MessageChannelResponce;

                    if (channelResponce == null || !channelResponce.Approve)
                    {
                        //если отказано завершаем работу
                        Console.WriteLine("connection {0} disapproved channels {1}", channelResponce == null ? Guid.Empty : channelResponce.Uid, string.Join(",", channelRequest.Channels));
                        Console.ReadKey();
                    }
                    else
                    {
                        //если разрешено начинаем получать сообщения из очереди
                        Console.WriteLine("connection {0} approved channels {1}", channelResponce.Uid, string.Join(",", channelRequest.Channels));
                        ReceiveMessages(client, stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            finally
            {
                Disconnect(client, stream);
            }
        }

        /// <summary>
        /// Получение произвольного сообщения с сервера
        /// </summary>
        private static IMessageBase ReceiveMessage(NetworkStream stream)
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
        /// Получение сообщений из очереди
        /// </summary>
        private void ReceiveMessages(TcpClient client, NetworkStream stream)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("await message");
                    var channelMessage = ReceiveMessage(stream) as MessageChannelMessage;

                    Thread.Sleep(100);

                    var channelMessageresponce = new MessageChannelMessageResponce();
                    byte[] data = MessageBase.ToBytes(channelMessageresponce);
                    stream.Write(data, 0, data.Length);

                    Thread.Sleep(500);

                    //TODO тут обработку при желании тоже можно вынести в отдельный поток
                    if (channelMessage != null)
                    {
                        Console.WriteLine(string.Format("cannel {0}: {1}", channelMessage.Name, channelMessage.Text));
                    }
                }
                catch
                {
                    //TODO внятное сообщение об ошибке

                    Disconnect(client, stream);
                    Console.WriteLine("connection aborted");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Завершение потока и отключение клиента
        /// </summary>
        private void Disconnect(TcpClient client, NetworkStream stream)
        {
            if (stream != null)
                stream.Close();

            if (client != null)
                client.Close();

            Environment.Exit(0);
        }
    }
}
