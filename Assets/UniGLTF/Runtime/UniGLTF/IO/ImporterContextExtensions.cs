using System.IO;
using UniGLTF.AltTask;
using UnityEngine;

namespace UniGLTF
{
    public static class ImporterContextExtensions
    {
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
