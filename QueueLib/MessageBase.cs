using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace QueueLib
{
    /// <summary>
    /// Интерфейс базового класса сообщений
    /// </summary>
    public interface IMessageBase
    {
        long TimeStamp { get; }
    }

    /// <summary>
    /// Базовый класс сообщений
    /// </summary>
    [Serializable]
    public abstract class MessageBase : IMessageBase
    {
        public MessageBase()
        {
            TimeStamp = DateTime.Now.Ticks;
        }

        public long TimeStamp { get; private set; }

        /// <summary>
        /// Конвертация сообщений в массив байтов
        /// </summary>
        public static byte[] ToBytes(IMessageBase message)
        {
            byte[] bytes;
            var formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, message);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        /// <summary>
        /// Конвертация массива байтов в сообщение
        /// </summary>
        public static IMessageBase FromBytes(byte[] bytes)
        {
            var formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                object obj = formatter.Deserialize(ms);
                return (IMessageBase)obj;
            }
        }
    }
}
