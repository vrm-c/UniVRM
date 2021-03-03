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
                if (task.Exception is AggregateException ae && ae.InnerExceptions.Count == 1)
                {
                    throw ae.InnerException;
                }
                else
                {
                    throw task.Exception;
                }
            }

#if VRM_DEVELOP
            Debug.Log(meassureTime.GetSpeedLog());
#endif
        }
    }
}
