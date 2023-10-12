namespace UniVRM10
{
    public readonly struct LookAtEyeDirection
    {
        /// <summary>
        /// Positive is right.
        /// Negative is left.
        ///
        /// </summary>
        public float Yaw { get; }

        /// <summary>
        /// Positive is upper.
        /// Negative is lower.
        ///
        /// Usually in z-forward y-up left coordinates, positive is lower.
        /// This is inverted because of following the vrm-1.0 specification.
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