namespace UniGLTF
{
    public interface IBlendShapeExportFilter
    {
        bool UseBlendShape(int blendShapeIndex, string relativePath);
    }

    public class DefualtBlendShapeExportFilter : IBlendShapeExportFilter
    {
        /// <summary>
        /// Export all blendshape
        /// </summary>
        /// <param name="blendShapeIndex"></param>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public bool UseBlendShape(int blendShapeIndex, string relativePath) => true;
    }
}
