namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// BlittablePointのポインタの配列
    /// </summary>
    public unsafe struct BlittablePoints
    {
        private readonly BlittablePoint* _points;
        public int Count { get; }

        public BlittablePoint this[int i]
        {
            get => _points[i];
            set => _points[i] = value;
        } 

        public BlittablePoints(BlittablePoint* points, int count)
        {
            _points = points;
            Count = count;
        }
    }
}