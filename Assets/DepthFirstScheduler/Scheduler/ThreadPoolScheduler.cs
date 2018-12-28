using System;

namespace DepthFirstScheduler
{
    public static partial class Scheduler
    {
        private static IScheduler threadPool;

        public static IScheduler ThreadPool
        {
            get { return threadPool ?? (threadPool = new ThreadPoolScheduler()); }
        }

        public class ThreadPoolScheduler : IScheduler
        {
            public void Enqueue(TaskChain item)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    if (item == null)
                    {
                        return;
                    }

                    while (true)
                    {
                        var status = item.Next();
                        if (status != ExecutionStatus.Continue)
                        {
                            break;
                        }
                    }

                });
            }

            public void Dispose()
            {
            }
        }
    }
}
