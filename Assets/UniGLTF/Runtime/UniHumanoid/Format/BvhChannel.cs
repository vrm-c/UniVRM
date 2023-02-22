namespace UniHumanoid
{
    public enum BvhChannel
    {
        Xposition,
        Yposition,
        Zposition,
        Xrotation,
        Yrotation,
        Zrotation,
    }

    public static class BvhChannelExtensions
    {
        public static string ToProperty(this BvhChannel ch)
        {
            switch (ch)
            {
                case BvhChannel.Xposition: return "localPosition.x";
                case BvhChannel.Yposition: return "localPosition.y";
                case BvhChannel.Zposition: return "localPosition.z";
                case BvhChannel.Xrotation: return "localEulerAnglesBaked.x";
                case BvhChannel.Yrotation: return "localEulerAnglesBaked.y";
                case BvhChannel.Zrotation: return "localEulerAnglesBaked.z";
            }

            throw new BvhException("no property for " + ch);
        }

        public static bool IsLocation(this BvhChannel ch)
        {
            switch (ch)
            {
                case BvhChannel.Xposition:
                case BvhChannel.Yposition:
                case BvhChannel.Zposition: return true;
                case BvhChannel.Xrotation:
                case BvhChannel.Yrotation:
                case BvhChannel.Zrotation: return false;
            }

            throw new BvhException("no property for " + ch);
        }
    }
}
