using System;
using System.Threading.Tasks;

namespace VRMShaders
{
    /// <summary>
    /// ImporterContext の 非同期実行 LoadAsync を補助する。
    /// この関数を経由して await すること。
    /// そうしないと、同期実行 Load 時にデッドロックに陥るかもしれない。
    /// (SynchronizationContext に Post された 継続が再開されない)
    /// </summary>
    public interface IAwaitCaller
    {
        /// <summary>
        /// フレームレートを維持するために１フレーム待つ
        /// </summary>
        /// <returns></returns>
        Task NextFrame();

        /// <summary>
        /// 非同期に実行して、終了を待つ
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task Run(Action action);

        /// <summary>
        /// 非同期に実行して、終了を待つ
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Run<T>(Func<T> action);
    }

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

    /// <summary>
    /// 非同期実行
    /// </summary>
    public sealed class TaskCaller : IAwaitCaller
    {
        public Task NextFrame()
        {
            return Task.Run(() =>
            {
                // Thread.Sleep(10);
            });
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
