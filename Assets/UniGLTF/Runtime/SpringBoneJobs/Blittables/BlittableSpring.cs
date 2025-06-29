using System;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// 1本の毛束を表すデータ型
    /// FastSpringBoneではこれを起点として並列化し、処理を行う
    /// </summary>
    [Serializable]
    public readonly struct BlittableSpring
    {
        public readonly BlittableSpan colliderSpan;
        public readonly BlittableSpan logicSpan;
        public readonly int centerTransformIndex;
        public readonly int transformIndexOffset;
        public readonly int modelIndex;
        
        public BlittableSpring(BlittableSpan colliderSpan = default, BlittableSpan logicSpan = default, int centerTransformIndex = 0, int transformIndexOffset = 0, int modelIndex = 0)
        {
            this.colliderSpan = colliderSpan;
            this.logicSpan = logicSpan;
            this.centerTransformIndex = centerTransformIndex;
            this.transformIndexOffset = transformIndexOffset;
            this.modelIndex = modelIndex;
        }
    }
}