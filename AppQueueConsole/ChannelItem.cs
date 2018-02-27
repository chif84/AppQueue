using System;
using System.Collections.Generic;
using System.Threading;

namespace AppQueueConsole
{
    /// <summary>
    /// Очередь сообщений
    /// </summary>
    internal class ChannelItem
    {
        #region Ctr

        public ChannelItem(string name)
        {
            Name = name;
            queue = new Queue<string>();
        }

        #endregion

        #region Private

        /// <summary>
        /// Внутренняя реализация очереди
        /// </summary>
        private Queue<string> queue;

        private readonly object queueLock = new object();

        #endregion

        /// <summary>
        /// Имя очереди
        /// </summary>
        public string Name { get; }

        public TcpItem Item { get; set; }

        /// <summary>
        /// Получить сообщение из очереди
        /// </summary>
        public string Get()
        {
            lock (queueLock)
            {
                try
                {
                    if (queue.Count == 0)
                        return null;

                    return queue.Dequeue();
                }
                catch(InvalidOperationException)
                {
                    //вообще не должно сюда попадать, но пока оставим
                    return null;
                }
            }
        }

        /// <summary>
        /// Получить сообщение из очереди
        /// </summary>
        public string Peek()
        {
            lock (queueLock)
            {
                try
                {
                    if (queue.Count == 0)
                        return null;

                    return queue.Peek();
                }
                catch (InvalidOperationException)
                {
                    //вообще не должно сюда попадать, но пока оставим
                    return null;
                }
            }
        }

        /// <summary>
        /// Положить сообщение в очередь
        /// </summary>
        public void Set(string item)
        {
            lock (queueLock)
            {
                queue.Enqueue(item);
                Item.NewItemEvent.Set();
            }
        }

    }
}
