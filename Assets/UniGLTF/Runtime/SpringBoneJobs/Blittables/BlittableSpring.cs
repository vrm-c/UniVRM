using System;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// 1本の毛束を表すデータ型
    /// FastSpringBoneではこれを起点として並列化し、処理を行う
    /// </summary>
    [Serializable]
    public struct BlittableSpring
    {
        public BlittableSpan colliderSpan;
        public BlittableSpan logicSpan;
        public int centerTransformIndex;
        public int transformIndexOffset;
        public int modelIndex;
    }
}