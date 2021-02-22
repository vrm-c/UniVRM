using System.IO;
using UniGLTF.AltTask;
using UnityEngine;

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
            var meassureTime = new ImporterContextSpeedLog();
            using (var queue = TaskQueue.Create())
            {
                var task = self.LoadAsync(meassureTime.MeasureTime);

                // 中断された await を消化する
                while (!task.IsCompleted)
                {
                    // execute synchronous
                    queue.ExecuteOneCallback();
                }
            }

#if VRM_DEVELOP
            Debug.Log(meassureTime.GetSpeedLog());
#endif
        }
    }
}
