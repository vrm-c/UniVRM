using System;
using System.Collections;
using System.Collections.Generic;
#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER)
using System.Threading.Tasks;
#endif

namespace DepthFirstScheduler
{
    public interface ISchedulable
    {
        /// <returns>実行が終了したか？Coroutineの実行が一回で終わらない場合がある</returns>
        ExecutionStatus Execute();
        Exception GetError();
        IScheduler Scheduler { get; }

        ISchedulable Parent { get; set; }
        void AddChild(ISchedulable child);
        IEnumerable<ISchedulable> Traverse();
    }

    public static class ISchedulableExtensions
    {
        public static ISchedulable GetRoot(this ISchedulable self)
        {
            var current = self;
            while (current.Parent != null)
            {
                current = current.Parent;
            }
            return current;
        }
    }

    public class NoParentException: Exception
    {              
        public NoParentException():base("No parent task can't ContinueWith or OnExecute. First AddTask")
        {
        }
    }

    public class Schedulable<T> : ISchedulable
    {
        List<ISchedulable> m_children = new List<ISchedulable>();
        public void AddChild(ISchedulable child)
        {
            child.Parent = this;
            m_children.Add(child);
        }
        public IEnumerable<ISchedulable> Traverse()
        {
            yield return this;

            foreach (var child in m_children)
            {
                foreach (var x in child.Traverse())
                {
                    yield return x;
                }
            }
        }

        public ISchedulable Parent
        {
            get;
            set;
        }

        public IScheduler Scheduler
        {
            get;
            private set;
        }

        public IFunctor<T> Func
        {
            get;
            private set;
        }

        public Exception GetError()
        {
            return Func.GetError();
        }

        public Schedulable()
        {
        }

        public Schedulable(IScheduler scheduler, IFunctor<T> func)
        {
            Scheduler = scheduler;
            Func = func;
        }

        public ExecutionStatus Execute()
        {
            if (Func == null)
            {
                return ExecutionStatus.Done;
            }
            return Func.Execute();
        }

        /// <summary>
        /// スケジュールされたタスクをすべて即時に実行する
        /// </summary>
        public void ExecuteAll()
        {
            foreach (var x in this.GetRoot().Traverse())
            {
                while (true)
                {
                    var status = x.Execute();
                    if (status != ExecutionStatus.Continue)
                    {
                        if (status == ExecutionStatus.Error)
                        {
                            throw x.GetError();
                        }
                        break;
                    }
                    // Coroutineタスクが継続している
                }
            }
        }

        public Schedulable<Unit> AddTask(IScheduler scheduler, Action pred)
        {
            return AddTask(scheduler, () => { pred(); return Unit.Default; });
        }

        public Schedulable<U> AddTask<U>(IScheduler scheduler, Func<U> pred)
        {
            var schedulable = new Schedulable<U>(scheduler, Functor.Create(() => Unit.Default, _ => pred()));
            AddChild(schedulable);
            return schedulable;
        }

        public Schedulable<T> AddCoroutine(IScheduler scheduler, Func<IEnumerator> starter)
        {
            var func = CoroutineFunctor.Create(() => default(T), _ => starter());
            var schedulable = new Schedulable<T>(scheduler, func);
            AddChild(schedulable);
            return schedulable;
        }

        public Schedulable<Unit> ContinueWith(IScheduler scheduler, Action<T> pred)
        {
            return ContinueWith(scheduler, t => { pred(t); return Unit.Default; });
        }

        public Schedulable<U> ContinueWith<U>(IScheduler scheduler, Func<T, U> pred)
        {
            if (Parent == null)
            {
                throw new NoParentException();
            }

            Func<T> getResult = null;
            if (Func != null)
            {
                getResult = Func.GetResult;
            }
            var func = Functor.Create(getResult, pred);
            var schedulable = new Schedulable<U>(scheduler, func);
            Parent.AddChild(schedulable);
            return schedulable;
        }

        public Schedulable<T> ContinueWithCoroutine(IScheduler scheduler, Func<IEnumerator> starter)
        {
            if (Parent == null)
            {
                throw new NoParentException();
            }

            var func = CoroutineFunctor.Create(() => default(T), _ => starter());
            var schedulable = new Schedulable<T>(scheduler, func);
            Parent.AddChild(schedulable);
            return schedulable;
        }

        public Schedulable<Unit> OnExecute(IScheduler scheduler, Action<Schedulable<Unit>> pred)
        {
            if (Parent == null)
            {
                throw new NoParentException();
            }

            Func<T> getResult = null;
            if (Func != null)
            {
                getResult = Func.GetResult;
            }

            var schedulable = new Schedulable<Unit>();
            schedulable.Func = Functor.Create(getResult, _ => { pred(schedulable); return Unit.Default; });
            Parent.AddChild(schedulable);
            return schedulable;
        }
    }

    public static class Schedulable
    {
        public static Schedulable<Unit> Create()
        {
            return new Schedulable<Unit>().AddTask(Scheduler.CurrentThread, () =>
            {
            });
        }
    }

    public static class SchedulableExtensions
    {
        public static void Subscribe<T>(
            this Schedulable<T> schedulable,
            IScheduler scheduler,
            Action<T> onCompleted,
            Action<Exception> onError)
        {
            schedulable.ContinueWith(scheduler, onCompleted);
            TaskChain.Schedule(schedulable.GetRoot(), onError);
        }

#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER)
        public static Task<T> ToTask<T>(this Schedulable<T> schedulable)
        {
            return ToTask(schedulable, Scheduler.MainThread);
        }

        public static Task<T> ToTask<T>(this Schedulable<T> schedulable, IScheduler scheduler)
        {
            var tcs = new TaskCompletionSource<T>();
            schedulable.Subscribe(scheduler, r => tcs.TrySetResult(r), ex => tcs.TrySetException(ex));
            return tcs.Task;
        }
#endif

    }
}
