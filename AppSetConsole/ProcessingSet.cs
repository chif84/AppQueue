using QueueLib;
using System;
using System.Net.Sockets;

namespace AppSetConsole
{
    /// <summary>
    /// Отправка сообщений
    /// </summary>
    internal class ProcessingSet
    {

        public void Processing()
        {
            while (true)
            {
                //вводим имя очереди
                Console.WriteLine("channel:");
                var ki = Console.ReadKey();
                Console.WriteLine();

                if (ki.Key == ConsoleKey.Escape)
                    break;

                var channel = 0;
                if (int.TryParse(Convert.ToString(ki.KeyChar), out channel))
                    SendToChannel(channel.ToString());
                else
                    Console.WriteLine("incorrect channel");
            }
        }

        /// <summary>
        /// Отправка сообщения в очередь
        /// </summary>
        private static void SendToChannel(string channel)
        {
            using (var client = new UdpClient())
            {
                try
                {
                    client.Connect(HostConstants.Host, HostConstants.Port);

                    //генерируем сообщение
                    var message = new MessageChannelMessage();
                    message.Name = channel;
                    message.Text = string.Format("i am message for channel {0} ({1})", message.Name, message.TimeStamp);

                    //отправляем сообщение
                    byte[] bytes = MessageBase.ToBytes(message);
                    client.Send(bytes, bytes.Length);

                    Console.WriteLine("message sended");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                }
            }
        }
    }
}
