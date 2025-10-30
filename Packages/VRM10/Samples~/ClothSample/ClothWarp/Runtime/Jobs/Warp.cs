namespace UniVRM10.ClothWarp.Jobs
{
    public struct ArrayRange
    {
        public int Start;
        public int End;
    }

    public struct WarpInfo
    {
        public ArrayRange PrticleRange;
        public ArrayRange ColliderGroupRefRange;
    }
}
