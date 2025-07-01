using System;
using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// 1本の毛束を表すデータ型
    /// FastSpringBoneではこれを起点として並列化し、処理を行う
    /// </summary>
    [Serializable]
    public readonly struct BlittableSpring
    {
        private readonly int4x2 _data;
        
        public BlittableSpan colliderSpan => new(_data.c0.x, _data.c0.y);
        public BlittableSpan logicSpan => new(_data.c0.z, _data.c0.w);
        public int centerTransformIndex => _data.c1.x;
        public int transformIndexOffset => _data.c1.y;
        public int modelIndex => _data.c1.z;
        
        public BlittableSpring(BlittableSpan colliderSpan = default, BlittableSpan logicSpan = default, int centerTransformIndex = 0, int transformIndexOffset = 0, int modelIndex = 0)
        {
            var c0 = new int4(colliderSpan.startIndex, colliderSpan.count, logicSpan.startIndex, logicSpan.count);
            var c1 = new int4(centerTransformIndex, transformIndexOffset, modelIndex, 0);
            _data = new int4x2(c0, c1);
        }
    }
}