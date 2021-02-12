using System.Threading.Tasks;
using UnityEngine;

namespace UniGLTF
{
    public struct GetTextureParam
    {
        public const string NORMAL_PROP = "_BumpMap";
        public const string METALLIC_GLOSS_PROP = "_MetallicGlossMap";
        public const string OCCLUSION_PROP = "_OcclusionMap";

        public readonly string TextureType;
        public readonly float MetallicFactor;
        public readonly ushort? Index0;
        public readonly ushort? Index1;
        public readonly ushort? Index2;
        public readonly ushort? Index3;
        public readonly ushort? Index4;
        public readonly ushort? Index5;

        public GetTextureParam(string textureType, float metallicFactor, int i0, int i1, int i2, int i3, int i4, int i5)
        {
            TextureType = textureType;
            MetallicFactor = metallicFactor;
            Index0 = (ushort)i0;
            Index1 = (ushort)i1;
            Index2 = (ushort)i2;
            Index3 = (ushort)i3;
            Index4 = (ushort)i4;
            Index5 = (ushort)i5;
        }

        public static GetTextureParam Create(int index)
        {
            return new GetTextureParam(default, default, index, default, default, default, default, default);
        }

        public static GetTextureParam CreateNormal(int index)
        {
            return new GetTextureParam(NORMAL_PROP, default, index, default, default, default, default, default);
        }

        public static GetTextureParam CreateMetallic(int index, float metallicFactor)
        {
            return new GetTextureParam(METALLIC_GLOSS_PROP, metallicFactor, index, default, default, default, default, default);
        }

        public static GetTextureParam CreateOcclusion(int index)
        {
            return new GetTextureParam(OCCLUSION_PROP, default, index, default, default, default, default, default);
        }
    }

    public delegate Task<Texture2D> GetTextureAsyncFunc(GetTextureParam param);
}
