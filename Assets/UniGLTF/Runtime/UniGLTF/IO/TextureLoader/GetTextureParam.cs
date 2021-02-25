using System;

namespace UniGLTF
{
    public struct GetTextureParam
    {
        public const string NORMAL_PROP = "_BumpMap";
        public const string METALLIC_GLOSS_PROP = "_MetallicGlossMap";
        public const string OCCLUSION_PROP = "_OcclusionMap";

        public readonly string Name;
        public readonly string TextureType;
        public readonly float MetallicFactor;
        public readonly ushort? Index0;
        public readonly ushort? Index1;
        public readonly ushort? Index2;
        public readonly ushort? Index3;
        public readonly ushort? Index4;
        public readonly ushort? Index5;

        public GetTextureParam(string name, string textureType, float metallicFactor, int i0, int i1, int i2, int i3, int i4, int i5)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException();
            }
            Name = name;

            TextureType = textureType;
            MetallicFactor = metallicFactor;
            Index0 = (ushort)i0;
            Index1 = (ushort)i1;
            Index2 = (ushort)i2;
            Index3 = (ushort)i3;
            Index4 = (ushort)i4;
            Index5 = (ushort)i5;
        }

        public static GetTextureParam Create(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, default, default, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam Create(glTF gltf, int index, string prop)
        {
            switch (prop)
            {
                case NORMAL_PROP:
                    return CreateNormal(gltf, index);

                case OCCLUSION_PROP:
                    return CreateOcclusion(gltf, index);

                case METALLIC_GLOSS_PROP:
                    return CreateMetallic(gltf, index, 1);

                default:
                    return Create(gltf, index);
            }
        }

        public static GetTextureParam CreateNormal(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name, NORMAL_PROP, default, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam CreateMetallic(glTF gltf, int textureIndex, float metallicFactor)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name + ".metallicRoughness", METALLIC_GLOSS_PROP, metallicFactor, textureIndex, default, default, default, default, default);
        }

        public static GetTextureParam CreateOcclusion(glTF gltf, int textureIndex)
        {
            var name = gltf.textures[textureIndex].name;
            return new GetTextureParam(name + ".occlusion", OCCLUSION_PROP, default, textureIndex, default, default, default, default, default);
        }
    }
}
