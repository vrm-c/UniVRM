using System;
using System.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// Runtime (Build 後と、Editor Playing) での非同期ロードを実現する AwaitCaller.
    /// WebGL など Thread が無いもの向け
    /// </summary>
    public sealed class RuntimeOnlyNoThreadAwaitCaller : IAwaitCaller
    {
        private readonly NextFrameTaskScheduler _scheduler;

        public RuntimeOnlyNoThreadAwaitCaller()
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
            try
            {
                action();
                return Task.FromResult<object>(null);
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        public Task<T> Run<T>(Func<T> action)
        {
            try
            {
                return Task.FromResult(action());
            }
            catch (Exception ex)
            {
                return Task.FromException<T>(ex);
            }
        }
    }
}