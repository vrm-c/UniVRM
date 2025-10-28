namespace UniVRM10.ClothWarp.Jobs
{
    public enum TransformType
    {
        Center,
        WarpRootParent,
        ClothWarp,
        Particle,
    }

    public static class TransformTypeExtensions
    {
        public static bool PositionInput(this TransformType t)
        {
            return t == TransformType.ClothWarp;
        }

        public static bool Movable(this TransformType t)
        {
            return t == TransformType.Particle;
        }

        public static bool Writable(this TransformType t)
        {
            switch (t)
            {
                case TransformType.ClothWarp:
                case TransformType.Particle:
                    return true;
                default:
                    return false;
            }
        }
    }
}