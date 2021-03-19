namespace VRM
{
    public static class VRMImporterContextExtensions
    {
        public static VRMMetaObject ReadMeta(this VRMImporterContext context, bool createThumbnail = false)
        {
            var task = context.ReadMetaAsync(default(UniGLTF.ImmediateCaller), createThumbnail);
            task.Wait();
            return task.Result;
        }
    }
}
