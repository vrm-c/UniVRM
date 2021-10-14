using System;
using System.Threading.Tasks;

namespace VRMShaders
{
    public sealed class RuntimeOnlyAwaitCaller : IAwaitCaller
    {
        private readonly NextFrameTaskScheduler _scheduler;

        public RuntimeOnlyAwaitCaller()
        {
            _scheduler = new NextFrameTaskScheduler();
        }

        public Task NextFrame()
        {
            var tcs = new TaskCompletionSource<object>();
            _scheduler.Enqueue(() => tcs.SetResult(default));
            return tcs.Task;
        }

        public Task Run(Action action)
        {
            return Task.Run(action);
        }

        public Task<T> Run<T>(Func<T> action)
        {
            return Task.Run(action);
        }
    }
}