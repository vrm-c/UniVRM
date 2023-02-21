using UnityEngine;

namespace UniVRM10
{
    public static class VRM10Retarget
    {
        public static void Retarget((INormalizedPoseProvider Pose, ITPoseProvider TPose) source, (INormalizedPoseApplicable Pose, ITPoseProvider TPose) sink)
        {
            foreach (var (head, parent) in sink.TPose.EnumerateBones())
            {
                var q = source.Pose.GetNormalizedLocalRotation(head, parent);
                sink.Pose.SetNormalizedLocalRotation(head, q);
            }

            // scaling hips position
            var scaling = sink.TPose.GetBoneTPoseWorldPosition(HumanBodyBones.LeftUpperLeg).y / source.TPose.GetBoneTPoseWorldPosition(HumanBodyBones.LeftUpperLeg).y;
            var delta = source.Pose.GetHipsPosition() - source.TPose.HipTPoseWorldPosition;
            sink.Pose.SetHipsPosition(sink.TPose.HipTPoseWorldPosition + delta * scaling);
        }

        public static void EnforceTPose((INormalizedPoseApplicable Pose, ITPoseProvider TPose) sink)
        {
            foreach (var (head, parent) in sink.TPose.EnumerateBones())
            {
                sink.Pose.SetNormalizedLocalRotation(head, Quaternion.identity);
            }

            sink.Pose.SetHipsPosition(sink.TPose.HipTPoseWorldPosition);
        }
    }
}
