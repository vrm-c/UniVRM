using UniJSON;

namespace UniVRM10
{
    internal static class MigrateVector3
    {
        /// <summary>
        /// VRM0は本来 (x, y, -z) と座標変換するべきところをしていない。
        /// 一方 VRM1 は (-x, y, z) と座標変換するように仕様を変更した。
        /// 
        /// VRM0 => VRM1 の変換では、 (-x, y, z) する。
        /// </summary>
        /// <param name="vrm0"></param>
        /// <returns></returns>
        public static float[] Migrate(JsonNode vrm0)
        {
            return new float[]
            {
                -vrm0["x"].GetSingle(),
                vrm0["y"].GetSingle(),
                vrm0["z"].GetSingle(),
            };
        }

        public static float[] Migrate(JsonNode parent, string key)
        {
            if (!parent.ContainsKey(key))
            {
                return new float[] { 0, 0, 0 };
            }
            return Migrate(parent[key]);
        }
    }
}
