using System;
using System.Collections;
using System.Collections.Generic;

namespace DepthFirstScheduler
{
    public enum ExecutionStatus
    {
        Unknown,
        Done,
        Continue, // coroutine or schedulable
        Error,
    }

    public interface IFunctor<T>
    {
        T GetResult();
        Exception GetError();
        ExecutionStatus Execute();
    }

    #region Functor
    public class Functor<T> : IFunctor<T>
    {
        T m_result;
        public T GetResult()
        {
            return m_result;
        }

        Exception m_error;
        public Exception GetError()
        {
            return m_error;
        }

        Action m_pred;
        public Functor(Func<T> func)
        {
            m_pred = () => m_result = func();
        }

        public ExecutionStatus Execute()
        {
            try
            {
                m_pred();
                return ExecutionStatus.Done;
            }
            catch (Exception ex)
            {
                m_error = ex;
                return ExecutionStatus.Error;
            }
        }
    }

    public static class Functor
    {
        /// <summary>
        /// 引数の型を隠蔽した実行器を生成する
        /// </summary>
        /// <typeparam name="S">引数の型</typeparam>
        /// <typeparam name="T">結果の型</typeparam>
        /// <param name="arg"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static Functor<T> Create<S, T>(Func<S> arg, Func<S, T> pred)
        {
            return new Functor<T>(() => pred(arg()));
        }
    }
    #endregion

    #region CoroutineFunctor
    public class CoroutineFunctor<T> : IFunctor<T>
    {
        T m_result;
        public T GetResult()
        {
            return m_result;
        }

        Exception m_error;
        public Exception GetError()
        {
            return m_error;
        }

        Func<T> m_arg;
        Func<T, IEnumerator> m_starter;
        Stack<IEnumerator> m_it;
        public CoroutineFunctor(Func<T> arg, Func<T, IEnumerator> starter)
        {
            m_arg = arg;
            m_starter = starter;
        }

        public ExecutionStatus Execute()
        {
            if (m_it == null)
            {
                m_result = m_arg();
                m_it = new Stack<IEnumerator>();
                m_it.Push(m_starter(m_result));
            }

            try
            {
                if (m_it.Count!=0)
                {
                    if (m_it.Peek().MoveNext())
                    {
                        var nested = m_it.Peek().Current as IEnumerator;
                        if (nested!=null)
                        {
                            m_it.Push(nested);
                        }
                    }
                    else
                    {
                        m_it.Pop();
                    }
                    return ExecutionStatus.Continue;
                }
                else
                {
                    return ExecutionStatus.Done;
                }

            }
            catch(Exception ex)
            {
                m_error = ex;
                return ExecutionStatus.Error;
            }
        }
    }

    public static class CoroutineFunctor
    {
        public static CoroutineFunctor<T> Create<T>(Func<T> arg, Func<T, IEnumerator> starter)
        {
            return new CoroutineFunctor<T>(arg, starter);
        }
    }
    #endregion

    /*
    public class SchedulableFunctor<T> : IFunctor<T>
    {
        Schedulable<T> m_schedulable;
        Func<Schedulable<T>> m_starter;
        TaskChain m_chain;

        public SchedulableFunctor(Func<Schedulable<T>> starter)
        {
            m_starter = starter;
        }

        public ExecutionStatus Execute()
        {
            if (m_chain == null)
            {
                m_schedulable = m_starter();
                m_chain = TaskChain.Schedule(m_schedulable, ex => m_error = ex);
            }

            return m_chain.Next();
        }

        Exception m_error;
        public Exception GetError()
        {
            return m_error;
        }

        public T GetResult()
        {
            return m_schedulable.Func.GetResult();
        }
    }

    public static class SchedulableFunctor
    {
        public static SchedulableFunctor<T> Create<T>(Func<Schedulable<T>> starter)
        {
            return new SchedulableFunctor<T>(starter);
        }
    }
    */
}
