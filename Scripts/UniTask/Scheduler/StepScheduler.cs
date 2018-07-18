namespace UniTask
{
    public static partial class Scheduler
    {
        private static StepScheduler mainThread;

        public static StepScheduler MainThread
        {
            get
            {
                if (mainThread != null) return mainThread;
                mainThread = new StepScheduler();
                MainThreadDispatcher.Initialize();
                return mainThread;
            }
        }

        public class StepScheduler : IScheduler
        {
            LockQueue<TaskChain> m_taskQueue = new LockQueue<TaskChain>();

            public void Enqueue(TaskChain item)
            {
                m_taskQueue.Enqueue(item);
            }

            TaskChain m_chain;

            public int UpdateAndGetTaskCount()
            {
                if (m_chain != null)
                {
                    var status = m_chain.Next();
                    if (status == ExecutionStatus.Continue)
                    {
                        // m_item継続中
                        return m_taskQueue.Count;
                    }
                    m_chain = null;
                }

                int count;
                m_chain = m_taskQueue.Dequeue(out count);
                return count;
            }

            public void Dispose()
            {
            }
        }
    }
}
