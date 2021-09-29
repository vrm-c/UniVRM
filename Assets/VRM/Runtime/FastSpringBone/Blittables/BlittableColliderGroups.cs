using UnityEngine;

namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// BlittableColliderGroupのポインタの配列
    /// </summary>
    public readonly unsafe struct BlittableColliderGroups
    {
        private readonly BlittableColliderGroup* _data;
        public int Length { get; }
        
        public BlittableColliderGroup this[int i] => _data[i];

        public void DrawGizmos()
        {
            for (var i = 0; i < Length; i++)
            {
                var group = this[i];
                var colliders = group.Colliders;
                for (var j = 0; j < colliders.Count; ++j)
                {
                    Gizmos.DrawWireSphere(group.Transform->WorldPosition, colliders[j].Radius);
                }
            }
        }

        public BlittableColliderGroups(BlittableColliderGroup* data, int length)
        {
            _data = data;
            Length = length;
        }
    }
}