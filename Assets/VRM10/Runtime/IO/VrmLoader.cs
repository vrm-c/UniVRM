using System.IO;
using VrmLib;
using UniGLTF;

namespace UniVRM10
{
    /// <summary>
    /// utility for load VrmLib Model from byte[]
    /// </summary>
    public static class VrmLoader
    {
        public static Model CreateVrmModel(GltfParser parser)
        {
            var storage = new Vrm10Storage(parser);
            var model = ModelLoader.Load(storage, Path.GetFileName(parser.TargetPath));
            model.ConvertCoordinate(Coordinates.Unity);
            return model;
        }
    }
}
