using System;
using System.Collections.Generic;
using System.Threading;


namespace DepthFirstScheduler
{
    /// <summary>
    /// http://blogs.msdn.com/b/toub/archive/2006/04/12/blocking-queues.aspx
    /// 
    /// 終了を通知するにはnullを投入する手が使える
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonitorQueue<T>
    {
        private Int32 _count = 0;
        public Int32 Count
        {
            get
            {
                return _count;
            }
        }

        private Queue<T> _queue = new Queue<T>();

        public T Dequeue()
        {
            lock (_queue)
            {
                while (_count <= 0) Monitor.Wait(_queue);
                _count--;
                return _queue.Dequeue();
            }
        }

        public void Enqueue(T data)
        {
            lock (_queue)
            {
                _queue.Enqueue(data);
                _count++;
                Monitor.Pulse(_queue);
            }
        }
    }
}
