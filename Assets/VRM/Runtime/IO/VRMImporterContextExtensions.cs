using VRMShaders;

namespace VRM
{
    public static class VRMImporterContextExtensions
    {
        public static VRMMetaObject ReadMeta(this VRMImporterContext context, bool createThumbnail = false)
        {
            var task = context.ReadMetaAsync(new ImmediateCaller(), createThumbnail);
            task.Wait();
            return task.Result;
        }
    }
}
