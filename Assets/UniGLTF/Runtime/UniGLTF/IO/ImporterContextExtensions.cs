using System;
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
            var task = self.LoadAsync(default(ImmediateCaller), meassureTime.MeasureTime);
            if (!task.IsCompleted)
            {
                throw new Exception();
            }
            if (task.IsFaulted)
            {
                throw new AggregateException(task.Exception);
            }

#if VRM_DEVELOP
            Debug.Log($"{self.Parser.TargetPath}: {meassureTime.GetSpeedLog()}");
#endif
        }
    }
}
