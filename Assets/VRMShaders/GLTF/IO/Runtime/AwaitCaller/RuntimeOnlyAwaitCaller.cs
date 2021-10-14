using System;
using System.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// Runtime (Build 後と、Editor Playing) での非同期ロードを実現する AwaitCaller.
    /// NOTE: 簡便に実装されたものなので、最適化の余地はある.
    /// </summary>
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