using System;
using System.Collections.Concurrent;

namespace VRMShaders
{
    internal sealed class TinyManagedTaskScheduler
    {
        private readonly ConcurrentQueue<(Action, Func<bool>)> _continuationQueue =
            new ConcurrentQueue<(Action, Func<bool>)>();
        private readonly ConcurrentQueue<(Action, Func<bool>)> _temporaryQueue =
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
                    _temporaryQueue.Enqueue(tuple);
                }
            }

            while (_temporaryQueue.TryDequeue(out var tuple))
            {
                _continuationQueue.Enqueue(tuple);
            }
        }

        public void Enqueue(Action continuation, Func<bool> canExecute)
        {
            _continuationQueue.Enqueue((continuation, canExecute));
        }
    }
}