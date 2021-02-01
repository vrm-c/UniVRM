namespace UniVRM10
{
    public readonly struct LookAtEyeDirection
    {
        public float LeftYaw { get; }
        public float LeftPitch { get; }
        public float RightYaw { get; }
        public float RightPitch { get; }

        public LookAtEyeDirection(float leftYaw, float leftPitch, float rightYaw, float rightPitch)
        {
            LeftYaw = leftYaw;
            LeftPitch = leftPitch;
            RightYaw = rightYaw;
            RightPitch = rightPitch;
        }
    }
}