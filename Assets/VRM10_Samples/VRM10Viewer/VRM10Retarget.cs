using UnityEngine;

namespace UniVRM10
{
    public static class VRM10Retarget
    {
        public static void Retarget((INormalizedPoseProvider Pose, ITPoseProvider TPose) source, (INormalizedPoseApplicable Pose, ITPoseProvider TPose) sink)
        {
            foreach (var (head, parent) in sink.TPose.EnumerateBoneParentPairs())
            {
                var q = source.Pose.GetNormalizedLocalRotation(head, parent);
                sink.Pose.SetNormalizedLocalRotation(head, q);
            }

            // scaling hips position
            var scaling = sink.TPose.GetBoneWorldPosition(HumanBodyBones.LeftUpperLeg).y / source.TPose.GetBoneWorldPosition(HumanBodyBones.LeftUpperLeg).y;
            var delta = source.Pose.GetRawHipsPosition() - source.TPose.GetBoneWorldPosition(HumanBodyBones.Hips);
            sink.Pose.SetRawHipsPosition(sink.TPose.GetBoneWorldPosition(HumanBodyBones.Hips) + delta * scaling);
        }

        public static void EnforceTPose((INormalizedPoseApplicable Pose, ITPoseProvider TPose) sink)
        {
            foreach (var (head, parent) in sink.TPose.EnumerateBoneParentPairs())
            {
                sink.Pose.SetNormalizedLocalRotation(head, Quaternion.identity);
            }

            sink.Pose.SetRawHipsPosition(sink.TPose.GetBoneWorldPosition(HumanBodyBones.Hips));
        }
    }
}
