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
            sink.Pose.SetHipsPosition(source.Pose.GetHipsPosition());
        }
    }
}
