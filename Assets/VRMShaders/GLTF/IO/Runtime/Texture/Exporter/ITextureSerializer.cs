using UnityEngine;

namespace VRMShaders
{
    /// <summary>
    /// Texture2D を入力として byte[] を得る機能
    /// </summary>
    public interface ITextureSerializer
    {
        /// <summary>
        /// Texture をファイルのバイト列そのまま出力してよいかどうか判断する。
        ///
        /// exportColorSpace はその Texture2D がアサインされる glTF プロパティの仕様が定める色空間を指定する。
        /// Runtime 出力では常に false が望ましい。
        /// </summary>
        bool CanExportAsEditorAssetFile(Texture texture, ColorSpace exportColorSpace);

        /// <summary>
        /// Texture2D から実際のバイト列を取得する。
        ///
        /// exportColorSpace はその Texture2D がアサインされる glTF プロパティの仕様が定める色空間を指定する。
        /// 具体的には Texture2D をコピーする際に、コピー先の Texture2D の色空間を決定するために使用する。
        /// </summary>
        (byte[] bytes, string mime) ExportBytesWithMime(Texture2D texture, ColorSpace exportColorSpace);

        /// <summary>
        /// エクスポートに使用したい Texture に対して、事前準備を行う。
        ///
        /// たとえば UnityEditor においては、Texture Asset の圧縮設定を OFF にしたりしたい。
        /// </summary>
        void ModifyTextureAssetBeforeExporting(Texture texture);
    }
}
