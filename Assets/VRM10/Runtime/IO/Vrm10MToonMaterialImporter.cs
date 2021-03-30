using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using VRMShaders;


namespace UniVRM10
{
    public class VrmMToonMaterialImporter
    {
        public static bool TryCreateParam(GltfParser parser, int i, out MaterialImportParam param)
        {
            if (!UniGLTF.Extensions.VRMC_materials_mtoon.GltfDeserializer.TryGet(parser.GLTF.materials[i].extensions,
                out UniGLTF.Extensions.VRMC_materials_mtoon.VRMC_materials_mtoon mtoon))
            {
                // fallback to gltf
                param = default;
                return false;
            }

            // use material.name, because material name may renamed in GltfParser.
            var name = parser.GLTF.materials[i].name;
            param = new MaterialImportParam(name, MToon.Utils.ShaderName);

            //

            return true;
        }
    }
}
