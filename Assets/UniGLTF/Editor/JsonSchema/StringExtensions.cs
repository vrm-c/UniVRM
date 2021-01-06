namespace UniGLTF.JsonSchema
{
    public static class StringExtensions
    {
        public static string ToUpperCamel(this string key)
        {
            return key.Substring(0, 1).ToUpper() + key.Substring(1);
        }
    }
}
