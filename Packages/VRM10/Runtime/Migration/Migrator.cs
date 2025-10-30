using System;

namespace UniVRM10
{
    public static class Migrator
    {
        /// <summary>
        /// マイグレーションの公開API
        /// 
        /// MigrationVrm とその関連実装は、internal で runtime import 専用
        /// </summary>
        /// <param name="vrm0bytes"></param>
        /// <param name="meta">(必須)外部から供給されるライセンス情報</param>
        /// <returns></returns>
        public static byte[] Migrate(byte[] vrm0bytes, VRM10ObjectMeta meta, Action<UniGLTF.glTF> modGltf = null)
        {
            if (meta == null)
            {
                throw new ArgumentNullException("meta");
            }
            foreach (var validation in meta.Validate())
            {
                if (!validation.CanExport)
                {
                    throw new ArgumentException(validation.Message);
                }
            }
            return MigrationVrm.Migrate(vrm0bytes, meta, modGltf);
        }
    }
}