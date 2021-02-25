using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace UniGLTF.AltTask
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

        public Awaitable<T> Task => new Awaitable<T>(_methodBuilder.Task);
    }

    [AsyncMethodBuilder(typeof(ExplicitTaskMethodBuilder<>))]
    public struct Awaitable<T> : IAwaitable<T>
    {
        private Task<T> _task;

        public Awaitable(Task<T> task)
        {
            _task = task;
            if (_task.Exception != null)
            {
                throw _task.Exception;
            }
        }

        public bool IsCompleted => _task.IsCompleted;
        public T Result => _task.Result;
        public Exception Exception => _task.Exception;

        public IAwaiter<T> GetAwaiter()
        {
            return new Awaiter<T>(this);
        }

        public void ContinueWith(Action action)
        {
            _task.ContinueWith((Task, _) =>
            {
                action();
            }, null);
        }

        public static Awaitable<T> Delay()
        {
            var task = Task.FromResult<T>(default);
            return new Awaitable<T>(task);
        }
    }

    public class Awaiter<T> : IAwaiter<T>
    {
        Awaitable<T> m_task;

        public bool IsCompleted
        {
            get
            {
                return m_task.IsCompleted;
            }
        }

        public T GetResult()
        {
            if (m_task.Exception != null)
            {
                throw m_task.Exception;
            }
            return m_task.Result;
        }

        public void OnCompleted(Action continuation)
        {
            var context = TaskQueue.Current;
            this.m_task.ContinueWith(() =>
            {
                context.Post(_ => continuation(), null);
            });
        }

        public Awaiter(Awaitable<T> task)
        {
            m_task = task;
        }
    }
}
