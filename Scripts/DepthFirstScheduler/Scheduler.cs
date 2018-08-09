using System;

namespace DepthFirstScheduler
{
    public interface IScheduler : IDisposable
    {
        void Enqueue(TaskChain item);
    }
}
