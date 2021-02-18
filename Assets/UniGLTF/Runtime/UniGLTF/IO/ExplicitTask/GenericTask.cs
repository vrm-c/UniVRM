using System;
using System.Collections.Generic;
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

    public delegate void EnequeueCallback(Action continuation);

    public class TaskQueue : IDisposable
    {
        [ThreadStatic]
        static EnequeueCallback s_EnequeueCallback;

        public static void EnequeueCurrent(Action continuation)
        {
            if (s_EnequeueCallback == null)
            {
                System.Threading.SynchronizationContext.Current.Post(_ =>
                {
                    continuation();
                }, null);
            }
            else
            {
                s_EnequeueCallback(continuation);
            }
        }

        EnequeueCallback m_backup;

        Queue<Action> m_tasks = new Queue<Action>();

        public static TaskQueue Create()
        {
            return new TaskQueue();
        }

        TaskQueue()
        {
            m_backup = s_EnequeueCallback;
            s_EnequeueCallback = x =>
            {
                m_tasks.Enqueue(x);
            };
        }

        public void Dispose()
        {
            s_EnequeueCallback = m_backup;
        }

        public bool ExecuteOneCallback()
        {
            if (m_tasks.Count == 0)
            {
                return false;
            }
            var task = m_tasks.Dequeue();
            task();
            return true;
        }
    }

    public class Awaiter<T> : IAwaiter<T>
    {
        Task<T> m_task;

        public bool IsCompleted
        {
            get
            {
                return m_task.IsCompleted;
            }
        }

        public T GetResult()
        {
            return m_task.Result;
        }

        public void OnCompleted(Action continuation)
        {
            TaskQueue.EnequeueCurrent(continuation);
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

        public bool IsCompleted => _task.IsCompleted;
    }

    public struct ThreadTask
    {
        public static ExplicitTask<T> RunThreadAsync<T>(Func<T> callback)
        {
            var tcs = new TaskCompletionSource<T>();
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var t = callback();
                    tcs.SetResult(t);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return new ExplicitTask<T>(tcs.Task);
        }
    }

    public struct Unit { }
}
