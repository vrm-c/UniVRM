using System;

namespace UniVRM10
{
    public static class MigrationApi
    {
        /// <summary>
        /// マイグレーションの公開API
        /// 
        /// MigrationVrm とその関連実装は、internal で runtime import 専用
        /// </summary>
        /// <param name="vrm0bytes"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static byte[] Migrate(byte[] vrm0bytes, VRM10ObjectMeta meta)
        {
            return MigrationVrm.Migrate(vrm0bytes, meta);
        }
    }
}