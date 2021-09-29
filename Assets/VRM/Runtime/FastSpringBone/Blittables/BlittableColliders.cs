namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// BlittableColliderのポインタの配列
    /// </summary>
    public unsafe struct BlittableColliders
    {
        private readonly BlittableCollider* _colliders;
        public int Count { get; }
        
        public BlittableCollider this[int i] => _colliders[i];

        public BlittableColliders(BlittableCollider* colliders, int count)
        {
            _colliders = colliders;
            Count = count;
        }
    }
}