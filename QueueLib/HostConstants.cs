using System.Net;

namespace QueueLib
{
    /// <summary>
    /// Данные хоста
    /// </summary>
    public static class HostConstants
    {
        /// <summary>
        /// Порт
        /// </summary>
        public const int Port = 9999;

        /// <summary>
        /// Локальный хост
        /// </summary>
        public static IPAddress HostAddress { get { return IPAddress.Parse(Host); } }

        /// <summary>
        /// Локальный хост
        /// </summary>
        public static string Host = "127.0.0.1";
    }
}
