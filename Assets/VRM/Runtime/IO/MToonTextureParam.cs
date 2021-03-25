using System;
using UniGLTF;
using UnityEngine;
using VRMShaders;


namespace VRM
{
    public static class MToonTextureParam
    {
        public static TextureImportParam Create(GltfParser parser, int index, Vector2 offset, Vector2 scale, string prop, float metallicFactor, float roughnessFactor)
        {
            switch (prop)
            {
                case TextureImportParam.NORMAL_PROP:
                    return GltfTextureImporter.CreateNormal(parser, index, offset, scale);

                default:
                    return GltfTextureImporter.CreateSRGB(parser, index, offset, scale);

                case TextureImportParam.OCCLUSION_PROP:
                case TextureImportParam.METALLIC_GLOSS_PROP:
                    throw new NotImplementedException();
            }
        }
    }
}
