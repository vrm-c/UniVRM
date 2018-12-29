using System;
using System.Linq;
using System.Collections.Generic;


namespace DepthFirstScheduler
{
    public enum ChainStatus
    {
        Unknown,
        Continue,
        Done,
        Error,
    }

    public class TaskChain
    {
        public IEnumerator<ISchedulable> Enumerator;
        public Action<Exception> OnError;
        public ChainStatus ChainStatus;

        public static TaskChain Schedule(ISchedulable schedulable, Action<Exception> onError)
        {
            var item = new TaskChain
            {
                Enumerator = schedulable.Traverse().GetEnumerator(),
                OnError = onError
            };

            if (item.Enumerator.MoveNext())
            {
                if (item.Enumerator.Current.Schedulder == null)
                {
                    // default
                    Scheduler.MainThread.Enqueue(item);
                }
                else
                {
                    item.Enumerator.Current.Schedulder.Enqueue(item);
                }
            }

            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ExecutionStatus Next()
        {
            if (this.ChainStatus == ChainStatus.Done
                || this.ChainStatus== ChainStatus.Error)
            {
                return ExecutionStatus.Done;
            }

            {
                var status = Enumerator.Current.Execute();
                if (status == ExecutionStatus.Error)
                {
                    ChainStatus = ChainStatus.Error;
                    OnError(Enumerator.Current.GetError());
                }
                if (status == ExecutionStatus.Continue)
                {
                    // 中断(coroutine)
                    ChainStatus = ChainStatus.Continue;
                    return ExecutionStatus.Continue;
                }
            }

            if (!Enumerator.MoveNext())
            {
                // 終了
                ChainStatus = ChainStatus.Done;
                return ExecutionStatus.Done;
            }

            if (Enumerator.Current.Schedulder != null)
            {
                // Scheduleして中断
                ChainStatus = ChainStatus.Continue;
                Enumerator.Current.Schedulder.Enqueue(this);
                return ExecutionStatus.Done;
            }

            return Next();
        }
    }
}
