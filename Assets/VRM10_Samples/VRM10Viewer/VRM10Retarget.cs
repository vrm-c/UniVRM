using UnityEngine;

namespace UniVRM10.VRM10Viewer
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
            var scaling = sink.TPose.GetBoneWorldPosition(HumanBodyBones.LeftUpperLeg).Value.y / source.TPose.GetBoneWorldPosition(HumanBodyBones.LeftUpperLeg).Value.y;
            var delta = source.Pose.GetRawHipsPosition() - source.TPose.GetBoneWorldPosition(HumanBodyBones.Hips).Value;
            sink.Pose.SetRawHipsPosition(sink.TPose.GetBoneWorldPosition(HumanBodyBones.Hips).Value + delta * scaling);
        }

        public static void EnforceTPose((INormalizedPoseApplicable Pose, ITPoseProvider TPose) sink)
        {
            foreach (var (bone, parent) in sink.TPose.EnumerateBoneParentPairs())
            {
                sink.Pose.SetNormalizedLocalRotation(bone, Quaternion.identity);
            }

            sink.Pose.SetRawHipsPosition(sink.TPose.GetBoneWorldPosition(HumanBodyBones.Hips).Value);
        }
    }
}
