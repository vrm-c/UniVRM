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
        /// Yaw of RightEye
        /// </summary>
        public float RightYaw { get; }
        
        /// <summary>
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
    }
}