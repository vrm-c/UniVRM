namespace UniVRM10
{
    public readonly struct LookAtEyeDirection
    {
        /// <summary>
        /// Yaw of LeftEye
        /// </summary>
        public float LeftYaw { get; }
        
        /// <summary>
        /// Pitch of LeftEye
        /// </summary>
        public float LeftPitch { get; }
        
        /// <summary>
        /// NOTE: 何故か使われていない
        /// Yaw of RightEye
        /// </summary>
        public float RightYaw { get; }
        
        /// <summary>
        /// NOTE: 何故か使われていない
        /// Pitch of RightEye
        /// </summary>
        public float RightPitch { get; }

        public LookAtEyeDirection(float leftYaw, float leftPitch, float rightYaw, float rightPitch)
        {
            LeftYaw = leftYaw;
            LeftPitch = leftPitch;
            RightYaw = rightYaw;
            RightPitch = rightPitch;
        }

        public static LookAtEyeDirection Multiply(LookAtEyeDirection a, float b)
        {
            return new LookAtEyeDirection(
                a.LeftYaw * b,
                a.LeftPitch * b,
                a.RightYaw * b,
                a.RightPitch * b
            );
        }
    }
}