using UniGLTF.Extensions.VRMC_vrm;

namespace UniVRM10
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// for SubAssetName
        /// </summary>
        /// <returns></returns>
        public static string ExtractName(this Expression expression)
        {
            ExpressionKey key =
            (expression.Preset == ExpressionPreset.custom)
            ? ExpressionKey.CreateCustom(expression.Name)
            : ExpressionKey.CreateFromPreset(expression.Preset)
            ;
            return $"Expression.{key}";
        }
    }
}
