using System;
using System.Collections.Generic;
using UniGLTF.Extensions.VRMC_vrm;
using UniJSON;

namespace UniVRM10
{
    /// <summary>
    /// Convert vrm0 binary to vrm1 binary. Json processing
    /// </summary>
    public static class Migration
    {
        static bool TryGet(this UniGLTF.glTFExtensionImport extensions, string key, out ListTreeNode<JsonValue> value)
        {
            foreach (var kv in extensions.ObjectItems())
            {
                if (kv.Key.GetString() == key)
                {
                    value = kv.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static byte[] Migrate(byte[] src)
        {
            var glb = UniGLTF.Glb.Parse(src);
            var json = glb.Json.Bytes.ParseAsJson();

            var gltf = UniGLTF.GltfDeserializer.Deserialize(json);
            if (!(gltf.extensions is UniGLTF.glTFExtensionImport import))
            {
                throw new Exception("not extensions");
            }
            if (!import.TryGet("VRM", out ListTreeNode<JsonValue> vrm))
            {
                throw new Exception("no vrm");
            }

            {
                var vrm1 = new VRMC_vrm();
                var f = new JsonFormatter();
                GltfSerializer.Serialize(f, vrm1);
                gltf.extensions = new UniGLTF.glTFExtensionExport().Add(VRMC_vrm.ExtensionName, f.GetStoreBytes());
            }

            ArraySegment<byte> vrm1Json = default;
            {
                var f = new JsonFormatter();
                UniGLTF.GltfSerializer.Serialize(f, gltf);
                vrm1Json = f.GetStoreBytes();
            }

            return UniGLTF.Glb.Create(vrm1Json, glb.Binary.Bytes).ToBytes();
        }
    }
}
