using UniGLTF;

namespace UniVRM10
{
    public static class ExtractKeyExtensions
    {
        static ExpressionKey Key(UniGLTF.Extensions.VRMC_vrm.Expression e)
        {
            if (e.Preset == UniGLTF.Extensions.VRMC_vrm.ExpressionPreset.custom)
            {
                return ExpressionKey.CreateCustom(e.Name);
            }
            else
            {
                return ExpressionKey.CreateFromPreset(e.Preset);
            }
        }

        // public static SubAssetKey ToExtractKey(this UniGLTF.Extensions.VRMC_vrm.Expression expression)
        // {
        //     var key = Key(expression);
        //     return new SubAssetKey(typeof(VRM10Expression), key.ToString(), ".asset");
        // }
    }
}

        /// <summary>
        /// for SubAssetName
        /// </summary>
        /// <returns></returns>
        // public static string ExtractKey(this Expression expression)
        // {
        //     ExpressionKey key =
        //     (expression.Preset == ExpressionPreset.custom)
        //     ? ExpressionKey.CreateCustom(expression.Name)
        //     : ExpressionKey.CreateFromPreset(expression.Preset)
        //     ;
        //     return key.ExtractKey;
        // }
