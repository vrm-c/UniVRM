namespace UniGLTF
{
    public sealed class ExtensionSupportFlags
    {
        /// <summary>
        /// KHR_texture_basisu 拡張を考慮するかどうか。
        /// </summary>
        public bool ConsiderKhrTextureBasisu { get; set; } = true;

        /// <summary>
        /// ファイルに含まれる "全ての" テクスチャの画像が Y 軸反転しているかどうか。
        /// 標準的なファイルは false。
        ///
        /// Unity は内部で画像を Y 軸反転させているため、あらかじめ Y 軸反転させておくほうが効率的であり、その場合に対応。
        /// https://docs.unity3d.com/Packages/com.unity.cloud.ktx@3.2/manual/creating-textures.html
        /// </summary>
        public bool IsAllTexturesYFlipped { get; set; }

        public void CopyValueFrom(ExtensionSupportFlags src)
        {
            ConsiderKhrTextureBasisu = src.ConsiderKhrTextureBasisu;
            IsAllTexturesYFlipped = src.IsAllTexturesYFlipped;
        }
    }
}