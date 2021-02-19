using System;
using System.Collections.Generic;
using System.Threading;

namespace UniGLTF.AltTask
{
    public class TaskQueue : SynchronizationContext, IDisposable
    {
        [ThreadStatic]
        static TaskQueue s_queue;

        public new static SynchronizationContext Current
        {
            get
            {
                if (s_queue == null)
                {
                    return System.Threading.SynchronizationContext.Current;
                }
                else
                {
                    return s_queue;
                }
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            m_tasks.Enqueue(() => d(state));
        }

        Queue<Action> m_tasks = new Queue<Action>();

        public static TaskQueue Create()
        {
            return new TaskQueue();
        }

        TaskQueue()
        {
            s_queue = this;
        }

        public void Dispose()
        {
            s_queue = null;
        }

        public bool ExecuteOneCallback()
        {
            if (m_tasks.Count == 0)
            {
                return false;
            }
            var task = m_tasks.Dequeue();
            task();
            return true;
        }
    }
}
