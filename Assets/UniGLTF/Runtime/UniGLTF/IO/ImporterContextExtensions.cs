using System;
using UnityEngine;

namespace UniGLTF
{
    public static class ImporterContextExtensions
    {
        /// <summary>
        /// Build unity objects from parsed gltf
        /// </summary>
        public static RuntimeGltfInstance Load(this ImporterContext self)
        {
            var meassureTime = new ImporterContextSpeedLog();
            var task = self.LoadAsync(new ImmediateCaller(), meassureTime.MeasureTime);
            if (!task.IsCompleted)
            {
                throw new Exception();
            }
            if (task.IsFaulted)
            {
                throw new AggregateException(task.Exception);
            }

            if (Symbols.VRM_DEVELOP)
            {
                Debug.Log($"{self.Data.TargetPath}: {meassureTime.GetSpeedLog()}");
            }

            return task.Result;
        }
    }
}
