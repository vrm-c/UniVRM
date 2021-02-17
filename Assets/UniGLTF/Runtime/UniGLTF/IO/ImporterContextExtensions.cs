using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UniGLTF
{
    public static class ImporterContextExtensions
    {
        /// <summary>
        /// ReadAllBytes, Parse, Create GameObject
        /// </summary>
        /// <param name="path">allbytes</param>
        public static void Load(this ImporterContext self, string path)
        {
            var bytes = File.ReadAllBytes(path);
            self.Load(path, bytes);
        }

        /// <summary>
        /// Parse, Create GameObject
        /// </summary>
        /// <param name="path">gltf or glb path</param>
        /// <param name="bytes">allbytes</param>
        public static void Load(this ImporterContext self, string path, byte[] bytes)
        {
            self.Parse(path, bytes);
            self.Load();
            self.Root.name = Path.GetFileNameWithoutExtension(path);
        }

        class TemporarySynchronizationContext : SynchronizationContext
        {
            Queue<Action> m_callbacks = new Queue<Action>();

            /// <summary>
            /// await して中断された継続がここにくる。
            /// 
            /// ex. await Task.Run(() =>{ /* ここ */ });
            /// </summary>
            /// <param name="d"></param>
            /// <param name="state"></param>
            public override void Post(SendOrPostCallback d, object state)
            {
                // Debug.Log($"Post");
                m_callbacks.Enqueue(() => d(state));
            }

            /// <summary>
            /// 一個デキューして実行する。
            /// Unityであれば毎フレームこのように消化しているであろう。
            /// 
            /// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Scripting/UnitySynchronizationContext.cs
            /// </summary>
            /// <returns></returns>
            public bool ExecuteOneCallback()
            {
                while (m_callbacks.Count == 0)
                {
                    // Debug.Log($"empty callbacks");
                    return false;
                }

                var callback = m_callbacks.Dequeue();
                callback();
                return true;
            }
        }

        /// <summary>
        /// Build unity objects from parsed gltf
        /// </summary>
        public static void Load(this ImporterContext self)
        {
            var tcs = new TemporarySynchronizationContext();

            // 元に戻すためバックアップ
            var backup = SynchronizationContext.Current;
            // 一時的に変更
            SynchronizationContext.SetSynchronizationContext(tcs);
            try
            {
                var task = self.LoadAsync();

                // 中断された await を消化する
                while (!task.IsCompleted)
                {
                    // execute synchronous
                    tcs.ExecuteOneCallback();
                }
            }
            finally
            {
                // 元に戻す
                SynchronizationContext.SetSynchronizationContext(backup);
            }
        }
    }
}
