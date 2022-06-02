using System.Collections.Generic;

namespace UniGLTF
{
    /// <summary>
    /// 過去バージョンに含まれていたが、廃止・統合された Shader のフォールバック情報
    /// </summary>
    public static class MaterialFallback
    {
        static Dictionary<string, string> s_fallbackShaders = new Dictionary<string, string>
        {
            {"VRM/UnlitTexture", "Unlit/Texture"},
            {"VRM/UnlitTransparent", "Unlit/Transparent"},
            {"VRM/UnlitCutout", "Unlit/Transparent Cutout"},
            {"UniGLTF/StandardVColor", UniGLTF.UniUnlit.UniUnlitUtil.ShaderName},
        };

        public static IReadOnlyDictionary<string, string> FallbackShaders => s_fallbackShaders;
    }
}
