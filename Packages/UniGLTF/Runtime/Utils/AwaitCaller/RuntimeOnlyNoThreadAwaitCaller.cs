using System;
using System.Threading.Tasks;

namespace UniGLTF
{
    /// <summary>
    /// Runtime (Build 後と、Editor Playing) での非同期ロードを実現する AwaitCaller.
    /// WebGL など Thread が無いもの向け
    /// </summary>
    public sealed class RuntimeOnlyNoThreadAwaitCaller : IAwaitCaller
    {
        private readonly NextFrameTaskScheduler _scheduler;
        private readonly float                  _timeoutInSeconds;
        private          float                  _lastTimeoutBaseTime;

        /// <summary>
        /// タイムアウト指定可能なコンストラクタ
        /// </summary>
        /// <param name="timeoutInSeconds">NextFrameIfTimedOutがタイムアウトと見なす時間(秒単位)</param>
        public RuntimeOnlyNoThreadAwaitCaller(float timeoutInSeconds = 1f / 1000f)
        {
            _scheduler = new NextFrameTaskScheduler();
            _timeoutInSeconds = timeoutInSeconds;
            ResetLastTimeoutBaseTime();
        }

        public Task NextFrame()
        {
            ResetLastTimeoutBaseTime();
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

        public Task NextFrameIfTimedOut() => CheckTimeout() ? NextFrame() : Task.CompletedTask;

        private void ResetLastTimeoutBaseTime() => _lastTimeoutBaseTime = 0f;

        private bool LastTimeoutBaseTimeNeedsReset => _lastTimeoutBaseTime == 0f;

        private bool CheckTimeout()
        {
            float t = UnityEngine.Time.realtimeSinceStartup;
            if (LastTimeoutBaseTimeNeedsReset)
            {
                _lastTimeoutBaseTime = t;
            }
            return (t - _lastTimeoutBaseTime) >= _timeoutInSeconds;
        }
    }
}