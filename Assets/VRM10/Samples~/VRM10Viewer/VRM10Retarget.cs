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
            var scaling = sink.TPose.GetWorldTransform(HumanBodyBones.LeftUpperLeg).Value.Translation.y / source.TPose.GetWorldTransform(HumanBodyBones.LeftUpperLeg).Value.Translation.y;
            var delta = source.Pose.GetRawHipsPosition() - source.TPose.GetWorldTransform(HumanBodyBones.Hips).Value.Translation;
            sink.Pose.SetRawHipsPosition(sink.TPose.GetWorldTransform(HumanBodyBones.Hips).Value.Translation + delta * scaling);
        }

        public static void EnforceTPose((INormalizedPoseApplicable Pose, ITPoseProvider TPose) sink)
        {
            foreach (var (bone, parent) in sink.TPose.EnumerateBoneParentPairs())
            {
                sink.Pose.SetNormalizedLocalRotation(bone, Quaternion.identity);
            }

            sink.Pose.SetRawHipsPosition(sink.TPose.GetWorldTransform(HumanBodyBones.Hips).Value.Translation);
        }
    }
}
