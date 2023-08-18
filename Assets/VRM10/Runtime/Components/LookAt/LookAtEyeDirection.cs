namespace UniVRM10
{
    public readonly struct LookAtEyeDirection
    {
        public float Yaw { get; }

        /// <summary>
        /// Pitch of LeftEye
        /// </summary>
        public float Pitch { get; }

        public LookAtEyeDirection(float yaw, float pitch)
        {
            Yaw = yaw;
            Pitch = pitch;
        }

        public static LookAtEyeDirection Multiply(LookAtEyeDirection a, float b)
        {
            return new LookAtEyeDirection(
                a.Yaw * b,
                a.Pitch * b
            );
        }
    }
}