namespace UniVRM10
{
    public class MigrationData
    {
        /// <summary>
        /// マイグレーション失敗など
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// vrm0 からマイグレーションした場合に、vrm0 版の meta 情報
        /// </summary>
        public readonly Migration.Vrm0Meta OriginalMetaBeforeMigration;

        /// <summary>
        /// Migration した結果のバイト列(デバッグ用)
        /// </summary>
        public readonly byte[] MigratedBytes;

        public MigrationData(string msg, Migration.Vrm0Meta meta = default, byte[] bytes = default)
        {
            Message = msg;
            OriginalMetaBeforeMigration = meta;
            MigratedBytes = bytes;
        }
    }
}
