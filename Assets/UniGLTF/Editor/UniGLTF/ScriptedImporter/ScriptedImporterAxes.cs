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
#if UNIGLTF_DEFAULT_AXES_X
                        return Axes.X;
#else
                        return Axes.Z;
#endif
                    }

                default: throw new System.NotImplementedException();
            }
        }
    }
}
