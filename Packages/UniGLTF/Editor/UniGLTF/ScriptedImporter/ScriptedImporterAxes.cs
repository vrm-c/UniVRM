namespace UniGLTF
{
    /// <summary>
    /// ScriptedImporter の Inspector 用の Axes 設定
    /// </summary>
    public enum ScriptedImporterAxes
    {
        Default,
        Z,
        X,
    }

    public static class ScriptedImporterAxesExtensions
    {
        public static Axes ToAxes(this ScriptedImporterAxes axis)
        {
            switch (axis)
            {
                case ScriptedImporterAxes.Z: return Axes.Z;
                case ScriptedImporterAxes.X: return Axes.X;
                case ScriptedImporterAxes.Default:
                    {
                        return UniGLTFPreference.GltfIOAxis;
                    }

                default: throw new System.NotImplementedException();
            }
        }
    }
}
