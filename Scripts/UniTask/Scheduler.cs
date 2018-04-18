using System;

namespace UniTask
{
    public interface IScheduler : IDisposable
    {
        void Enqueue(TaskChain item);
    }
}
