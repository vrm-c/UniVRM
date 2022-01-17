using System;

namespace UniGLTF
{
    public readonly struct ExportProgress
    {
        public readonly string Title;
        public readonly string Message;
        public readonly float Progress;

        public ExportProgress(string title, string message, float progress)
        {
            Title = title;
            Message = message;
            Progress = progress;
        }
    }

    public class NullProgress : IProgress<ExportProgress>
    {
        public void Report(ExportProgress value)
        {
        }
    }
}
