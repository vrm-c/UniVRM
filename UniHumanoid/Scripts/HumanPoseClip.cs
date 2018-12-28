using UnityEngine;


namespace UniHumanoid
{
    public class HumanPoseClip : ScriptableObject
    {
        public const string TPoseResourcePath = "T-Pose.pose";

        public Vector3 bodyPosition;

        public Quaternion bodyRotation;

        public float[] muscles;

        public HumanPose GetPose()
        {
            return new HumanPose
            {
                bodyPosition = bodyPosition,
                bodyRotation = bodyRotation,
                muscles = muscles
            };
        }

        public void ApplyPose(ref HumanPose pose)
        {
            bodyPosition = pose.bodyPosition;
            bodyRotation = pose.bodyRotation;
            muscles = (float[])pose.muscles.Clone();
        }
    }
}
