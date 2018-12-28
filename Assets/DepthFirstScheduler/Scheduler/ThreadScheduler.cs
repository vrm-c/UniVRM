using System;
using System.Threading;

namespace DepthFirstScheduler
{
    public static partial class Scheduler
    {
        private static IScheduler singleWorkerThread;

        public static IScheduler SingleWorkerThread
        {
            get { return singleWorkerThread ?? (singleWorkerThread = new ThreadScheduler()); }
        }

        public class ThreadScheduler : IScheduler
        {
            MonitorQueue<TaskChain> m_queue = new MonitorQueue<TaskChain>();

            Thread m_thread;

            public ThreadScheduler()
            {
                // start worker thread
                m_thread = new Thread(new ParameterizedThreadStart(Worker));
                m_thread.Start(m_queue);
            }

            static void Worker(Object arg)
            {
                MonitorQueue<TaskChain> queue = (MonitorQueue<TaskChain>)arg;
                while (true)
                {
                    var chain = queue.Dequeue();
                    if (chain == null)
                    {
                        break;
                    }

                    while (true)
                    {
                        var status = chain.Next();
                        if (status != ExecutionStatus.Continue)
                        {
                            break;
                        }
                    }
                }

                // end
            }

            public void Enqueue(TaskChain item)
            {
                m_queue.Enqueue(item);
            }

            #region IDisposable Support

            private bool disposedValue = false; // 重複する呼び出しを検出するには

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                        if (m_thread != null)
                        {
                            m_queue.Enqueue(null);
                            m_thread.Join();
                            m_thread = null;
                        }
                    }

                    // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                    // TODO: 大きなフィールドを null に設定します。

                    disposedValue = true;
                }
            }

            // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
            // ~ThreadScheduler() {
            //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            //   Dispose(false);
            // }

            // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
            public void Dispose()
            {
                // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
                Dispose(true);
                // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
                // GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}
