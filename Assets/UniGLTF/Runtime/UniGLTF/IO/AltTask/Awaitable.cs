using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace UniGLTF.AltTask
{
    public interface IAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }
        void GetResult();
    }

    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public struct AwaitableMethodBuilder
    {
        // https://referencesource.microsoft.com/#mscorlib/system/runtime/compilerservices/AsyncMethodBuilder.cs
        private AsyncTaskMethodBuilder _methodBuilder;

        public static AwaitableMethodBuilder Create() =>
            new AwaitableMethodBuilder { _methodBuilder = AsyncTaskMethodBuilder.Create() };

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
        public void SetResult()
        {
            _methodBuilder.SetResult();
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

        public Awaitable Task => new Awaitable(_methodBuilder.Task);
    }

    [AsyncMethodBuilder(typeof(AwaitableMethodBuilder))]
    public struct Awaitable : IAwaitable
    {
        private Task _task;

        public Awaitable(Task task)
        {
            _task = task;
        }

        public bool IsCompleted => _task.IsCompleted;

        public IAwaiter GetAwaiter()
        {
            return new Awaiter(this);
        }

        public void ContinueWith(Action action)
        {
            _task.ContinueWith((Task, _) =>
            {
                action();
            }, null);
        }

        public static Awaitable Delay()
        {
            var task = Task.FromResult<object>(default);
            return new Awaitable(task);
        }

        public static Awaitable<T> Run<T>(Func<T> action)
        {
            return new Awaitable<T>(Task.Run(action));
        }

        public static Awaitable Run<T>(Action action)
        {
            return new Awaitable(Task.Run(action));
        }

        public static Awaitable<T> FromResult<T>(T result)
        {
            return new Awaitable<T>(Task.FromResult(result));
        }
    }

    public class Awaiter : IAwaiter
    {
        Awaitable m_task;

        public bool IsCompleted
        {
            get
            {
                return m_task.IsCompleted;
            }
        }

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            var context = TaskQueue.Current;
            this.m_task.ContinueWith(() =>
            {
                context.Post(_ => continuation(), null);
            });
        }

        public Awaiter(Awaitable task)
        {
            m_task = task;
        }
    }
}
