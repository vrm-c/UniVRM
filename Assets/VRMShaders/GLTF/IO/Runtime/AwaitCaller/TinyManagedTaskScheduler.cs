using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMShaders
{
    internal sealed class TinyManagedTaskScheduler
    {
        private readonly ConcurrentQueue<(Action, Func<bool>)> _continuationQueue =
            new ConcurrentQueue<(Action, Func<bool>)>();

        public void ManagedUpdate()
        {
            while (_continuationQueue.TryDequeue(out var tuple))
            {
                var (continuation, canExecute) = tuple;

                if (canExecute())
                {
                    continuation();
                }
                else
                {
                    _continuationQueue.Enqueue(tuple);
                }
            }
        }

        public void Enqueue(Action continuation, Func<bool> canExecute)
        {
            _continuationQueue.Enqueue((continuation, canExecute));
        }
    }
}