using UnityEngine;

namespace VRMShaders
{
    public interface ITextureExporter
    {
        int ExportSRGB(Texture src);
        int ExportLinear(Texture src);
        int ExportMetallicSmoothnessOcclusion(Texture metallicSmoothTexture, float smoothness, Texture occlusionTexture);
        int ExportNormal(Texture src);
    }
}
