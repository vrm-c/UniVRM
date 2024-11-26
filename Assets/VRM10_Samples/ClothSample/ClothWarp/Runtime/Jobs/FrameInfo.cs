using UnityEngine;

namespace UniVRM10.ClothWarp.Jobs
{
    public struct FrameInfo
    {
        public readonly float DeltaTime;
        public readonly float SqDeltaTime;
        public readonly Vector3 Force; // += env.External / time.DeltaTime;            

        public FrameInfo(float deltaTime, Vector3 force)
        {
            DeltaTime = deltaTime;
            SqDeltaTime = deltaTime * deltaTime;
            Force = force;
        }
    }
}