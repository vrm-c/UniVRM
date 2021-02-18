using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public sealed class AsyncMethodBuilderAttribute : Attribute
    {
        public AsyncMethodBuilderAttribute(Type builderType)
        {
            BuilderType = builderType;
        }

        public Type BuilderType { get; }
    }
}

namespace UniGLTF.ExplicitTask
{
    public interface IAwaiter<out T> : INotifyCompletion
    {
        bool IsCompleted { get; }
        T GetResult();
    }

    public interface IAwaitable<out T>
    {
        IAwaiter<T> GetAwaiter();
    }

    public class Awaiter<T> : IAwaiter<T>
    {
        Task<T> m_task;

        public bool IsCompleted => m_task.IsCompleted;

        public T GetResult()
        {
            return m_task.Result;
        }

        public void OnCompleted(Action continuation)
        {
            continuation();
        }

        public Awaiter(Task<T> task)
        {
            m_task = task;
        }
    }

    public struct ExplicitTaskMethodBuilder<T>
    {
        // https://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs
        private AsyncTaskMethodBuilder<T> _methodBuilder;

        public static ExplicitTaskMethodBuilder<T> Create() =>
            new ExplicitTaskMethodBuilder<T> { _methodBuilder = AsyncTaskMethodBuilder<T>.Create() };

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.Start(ref stateMachine);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            _methodBuilder.SetStateMachine(stateMachine);
        }
        public void SetException(Exception exception)
        {
            _methodBuilder.SetException(exception);
        }
        public void SetResult(T result)
        {
            _methodBuilder.SetResult(result);
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        public ExplicitTask<T> Task => new ExplicitTask<T>(_methodBuilder.Task);
    }

    [AsyncMethodBuilder(typeof(ExplicitTaskMethodBuilder<>))]
    public struct ExplicitTask<T> : IAwaitable<T>
    {
        private Task<T> _task;

        public ExplicitTask(Task<T> task) => _task = task;

        public static ExplicitTask<T> Delay()
        {
            var task = Task.FromResult<T>(default);
            return new ExplicitTask<T>(task);
        }

        public T Result => _task.Result;

        public IAwaiter<T> GetAwaiter()
        {
            return new Awaiter<T>(_task);
        }
    }

    public struct Unit { }
}
