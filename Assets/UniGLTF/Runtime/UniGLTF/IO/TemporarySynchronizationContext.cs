using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// await で post される先を１次的に乗っ取る
    /// </summary>
    public class TemporarySynchronizationContext : SynchronizationContext
    {
        Queue<Action> m_callbacks = new Queue<Action>();

        public override void Post(SendOrPostCallback d, object state)
        {
            // Debug.Log($"Post");
            m_callbacks.Enqueue(() => d(state));
        }

        public bool ExecuteOneCallback()
        {
            while (m_callbacks.Count == 0)
            {
                // Debug.Log($"empty callbacks");
                return false;
            }

            var callback = m_callbacks.Dequeue();
            callback();
            return true;
        }

        class Hijacker : IDisposable
        {
            SynchronizationContext m_backup;
            public Hijacker(SynchronizationContext context)
            {
                m_backup = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(context);
            }

            public void Dispose()
            {
                SynchronizationContext.SetSynchronizationContext(m_backup);
            }
        }

        public IDisposable Hijack()
        {
            return new Hijacker(this);
        }
    }
}
