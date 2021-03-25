using System;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace VRM
{
    public static class VRMTextureParam
    {
        public static TextureImportParam Create(GltfParser parser, int index, Vector2 offset, Vector2 scale, string prop, float metallicFactor, float roughnessFactor)
        {
            switch (prop)
            {
                case TextureImportParam.NORMAL_PROP:
                    return TextureFactory.CreateNormal(parser, index, offset, scale);

                default:
                    return TextureFactory.CreateSRGB(parser, index, offset, scale);

                case TextureImportParam.OCCLUSION_PROP:
                case TextureImportParam.METALLIC_GLOSS_PROP:
                    throw new NotImplementedException();
            }
        }
    }
}
