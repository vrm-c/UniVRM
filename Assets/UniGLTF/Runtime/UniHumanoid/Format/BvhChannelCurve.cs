namespace UniHumanoid
{
    public class BvhChannelCurve
    {
        public float[] Keys
        {
            get;
            private set;
        }

        public BvhChannelCurve(int frameCount)
        {
            Keys = new float[frameCount];
        }

        public void SetKey(int frame, float value)
        {
            Keys[frame] = value;
        }
    }
}
