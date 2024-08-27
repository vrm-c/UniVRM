namespace UniGLTF
{
    public class ImporterContextSettings
    {
        public bool LoadAnimation { get; }
        public Axes InvertAxis { get; }

        /// <summary>
        /// ImporterContextの設定を指定する。
        /// </summary>
        /// <param name="loadAnimation">アニメーションをインポートする場合はtrueを指定(初期値はtrue)</param>
        /// <param name="invertAxis">GLTF から Unity に変換するときに反転させる軸を指定(初期値はAxes.Z)</param>
        public ImporterContextSettings(bool loadAnimation = true, Axes invertAxis = Axes.Z)
        {
            LoadAnimation = loadAnimation;
            InvertAxis = invertAxis;
        }
    }
}