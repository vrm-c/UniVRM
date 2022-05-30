using System;
using UnityEngine.Serialization;

namespace UniVRM10.FastSpringBones.Blittables
{
    /// <summary>
    /// 1本の毛束を表すデータ型
    /// </summary>
    [Serializable]
    public struct BlittableSpring
    {
        public BlittableSpan colliderSpan;
        public BlittableSpan logicSpan;
        public int centerTransformIndex;
        public int transformIndexOffset;
    }
}