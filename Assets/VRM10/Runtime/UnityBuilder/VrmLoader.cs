using System;
using System.IO;
using VrmLib;
using UniJSON;
using UniGLTF;

namespace UniVRM10
{
    /// <summary>
    /// utility for load VrmLib Model from byte[]
    /// </summary>
    public static class VrmLoader
    {
        // TODO:
        const string VRM0X_LICENSE_URL = "https://vrm-consortium.org/";

        /// <summary>
        /// Load VRM10 or VRM0x from path
        /// </summary>
        public static Model CreateVrmModel(string path)
        {
            var bytes = File.ReadAllBytes(path);
            return CreateVrmModel(bytes, new FileInfo(path));
        }

        public static Model CreateVrmModel(byte[] bytes, FileInfo path)
        {
            var parser = new GltfParser();
            parser.Parse(path.FullName, bytes);
            var storage = new Vrm10Storage(parser);
            var model = ModelLoader.Load(storage, path.Name);
            model.ConvertCoordinate(Coordinates.Unity);
            return model;
        }
    }
}
