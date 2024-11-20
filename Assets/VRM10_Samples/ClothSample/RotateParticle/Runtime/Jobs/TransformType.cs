namespace RotateParticle.Jobs
{
    public enum TransformType
    {
        Center,
        WarpRootParent,
        WarpRoot,
        Particle,
    }

    public static class TransformTypeExtensions
    {
        public static bool PositionInput(this TransformType t)
        {
            return t == TransformType.WarpRoot;
        }

        public static bool Movable(this TransformType t)
        {
            return t == TransformType.Particle;
        }

        public static bool Writable(this TransformType t)
        {
            switch (t)
            {
                case TransformType.WarpRoot:
                case TransformType.Particle:
                    return true;
                default:
                    return false;
            }
        }
    }
}