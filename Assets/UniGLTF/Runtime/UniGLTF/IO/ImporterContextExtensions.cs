using System;
using System.Collections;
using System.IO;
using UnityEngine;
using DepthFirstScheduler;
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

        /// <summary>
        /// Build unity objects from parsed gltf
        /// </summary>
        public static void Load(this ImporterContext self)
        {
            var schedulable = self.LoadAsync();
            schedulable.ExecuteAll();
        }

        public static IEnumerator LoadCoroutine(this ImporterContext self, Action<Exception> onError = null)
        {
            return self.LoadCoroutine(() => { }, onError);
        }

        public static IEnumerator LoadCoroutine(this ImporterContext self, Action onLoaded, Action<Exception> onError = null)
        {
            if (onLoaded == null)
            {
                onLoaded = () => { };
            }

            if (onError == null)
            {
                onError = Debug.LogError;
            }

            var schedulable = self.LoadAsync();
            foreach (var x in schedulable.GetRoot().Traverse())
            {
                while (true)
                {
                    var status = x.Execute();
                    if (status != ExecutionStatus.Continue)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            onLoaded();
        }

        public static void LoadAsync(this ImporterContext self, Action onLoaded, Action<Exception> onError = null)
        {
            if (onError == null)
            {
                onError = Debug.LogError;
            }

            self.LoadAsync()
                .Subscribe(Scheduler.MainThread,
                _ => onLoaded(),
                onError
                );
        }

#if ((NET_4_6 || NET_STANDARD_2_0) && UNITY_2017_1_OR_NEWER && !UNITY_WEBGL)
        public static async Task<GameObject> LoadAsyncTask(this ImporterContext self)
        {
            await self.LoadAsync().ToTask();
            return self.Root;
        }
#endif
    }
}
