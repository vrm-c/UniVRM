using System;
using System.IO;
using VrmLib;
using UniJSON;

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
            if (!Glb.TryParse(bytes, out Glb glb, out Exception ex))
            {
                throw ex;
            }

            var json = glb.Json.Bytes.ParseAsJson();

            var extensions = json["extensions"];

            foreach (var kv in extensions.ObjectItems())
            {
                switch (kv.Key.GetString())
                {
                    // case "VRM":
                    //     {
                    //         var storage = new Vrm10Storage(glb.Json.Bytes, glb.Binary.Bytes);
                    //         var model = ModelLoader.Load(storage, path.Name);
                    //         model.ConvertCoordinate(Coordinates.Unity);
                    //         return model;
                    //     }

                    case "VRMC_vrm":
                        {
                            var storage = new Vrm10Storage(glb.Json.Bytes, glb.Binary.Bytes);
                            var model = ModelLoader.Load(storage, path.Name);
                            model.ConvertCoordinate(Coordinates.Unity);
                            return model;
                        }
                }
            }

            // this is error
            // throw new NotImplementedException();
            return null;
        }
    }
}
