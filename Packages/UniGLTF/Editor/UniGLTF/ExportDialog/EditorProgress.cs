using System;
using UnityEditor;

namespace UniGLTF
{
    public class EditorProgress : IProgress<ExportProgress>
    {
        public void Report(ExportProgress value)
        {
            EditorUtility.DisplayProgressBar(value.Title, value.Message, value.Progress);
        }
    }
}
