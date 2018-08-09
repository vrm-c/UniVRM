using System;
using System.Collections;
using System.Collections.Generic;

namespace DepthFirstScheduler
{
    public static partial class Scheduler
    {
        private static IScheduler currentThread;

        public static IScheduler CurrentThread
        {
            get { return currentThread ?? (currentThread = new CurrentThreadScheduler()); }
        }

        public class CurrentThreadScheduler : IScheduler
        {
            [ThreadStatic]
            private static Queue<TaskChain> queue;

            private static Queue<TaskChain> GetQueue()
            {
                return queue;
            }

            private static void SetQueue(Queue<TaskChain> newQueue)
            {
                queue = newQueue;
            }

            public void Enqueue(TaskChain item)
            {
                var q = GetQueue();

                if (q == null)
                {
                    q = new Queue<TaskChain>(5);
                    q.Enqueue(item);
                    SetQueue(q);

                    try
                    {
                        Trampoline.Run(q);
                    }
                    finally
                    {
                        SetQueue(null);
                    }
                }
                else
                {
                    q.Enqueue(item);
                }
            }

            #region IDisposable Support

            private bool disposedValue = false;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        var queue = GetQueue();
                        if (queue != null) queue.Clear();
                        SetQueue(null);
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }

            #endregion
        }

        static class Trampoline
        {
            public static void Run(Queue<TaskChain> queue)
            {
                while (queue.Count > 0)
                {
                    var chain = queue.Dequeue();

                    while (true)
                    {
                        var status = chain.Next();
                        if (status != ExecutionStatus.Continue)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
