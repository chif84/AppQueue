using System;
using System.ServiceProcess;
using System.Threading;

namespace AppQueueConsole
{
    class Program
    {
        /// <summary>
        /// Имя сервиса
        /// </summary>
        public const string ServiceName = "AppQueueService";

        public static void Start(string[] args)
        {
            TcpList tcpList = null;
            UdpItem udpItem = null;

            try
            {
                //формируем список очередей
                var channelList = new ChannelList();

                //запускаем прослушивание tcp
                tcpList = new TcpList(channelList);
                var tcpListThread = new Thread(new ThreadStart(tcpList.ListenList));
                tcpListThread.Start();

                //запускаем прослушивание udp
                udpItem = new UdpItem(channelList);
                var udpItemThread = new Thread(new ThreadStart(udpItem.Listen));
                udpItemThread.Start();
            }
            catch (Exception ex)
            {
                if (tcpList != null)
                    tcpList.DisconnectList();

                Console.WriteLine(ex.Message);
            }
        }

        public static void Stop()
        {
            
        }

        static void Main(string[] args)
        {
            if (Environment.UserInteractive)
            {
                //запуск в консоли
                Start(args);
                Stop();
            }
            else
            {
                //запуск в сервисе
                using (var service = new AppQueueService())
                {
                    ServiceBase.Run(service);
                }
            }
        }
    }
}
