using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// ImporterContext層の設定はここに。
    /// さすれば、VRMとVRM10を変えずに設定を追加できる。
    /// </summary>
    public class ImporterContextSettings
    {
        public bool LoadAnimation { get; }
        public Axes InvertAxis { get; }
        public ImportedTexturesAccessibility ImportedTexturesAccessibility { get; }

        /// <summary>
        /// ImporterContextの設定を指定する。
        /// </summary>
        /// <param name="loadAnimation">アニメーションをインポートする場合はtrueを指定(初期値はtrue)</param>
        /// <param name="invertAxis">GLTF から Unity に変換するときに反転させる軸を指定(初期値はAxes.Z)</param>
        /// <param name="textureIsReadable">textureのbitmapにアクセスするか(初期値はEditorの場合はtrue。それ以外はfalse)</param>
        public ImporterContextSettings(
            bool loadAnimation = true,
            Axes invertAxis = Axes.Z,
            ImportedTexturesAccessibility importedTexturesAccessibility = ImportedTexturesAccessibility.Auto)
        {
            LoadAnimation = loadAnimation;
            InvertAxis = invertAxis;
            ImportedTexturesAccessibility = importedTexturesAccessibility;
        }
    }
}