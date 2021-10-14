using System;
using System.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// 同期実行
    /// </summary>
    public sealed class ImmediateCaller : IAwaitCaller
    {
        public Task NextFrame()
        {
            return Task.FromResult<object>(null);
        }

        public Task Run(Action action)
        {
            action();
            return Task.FromResult<object>(null);
        }

        public Task<T> Run<T>(Func<T> action)
        {
            return Task.FromResult(action());
        }
    }
}