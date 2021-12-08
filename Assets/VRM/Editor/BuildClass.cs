using UnityEditor;

namespace VRM
{
    public static class BuildClass
    {
        /// <summary>
        /// dummy build for CI
        /// </summary>
        public static void Build()
        {
            var scenes = new string[]{
                "./Assets/VRM_Samples/SimpleViewer/SimpleViewer.unity",
            };

            var report = BuildPipeline.BuildPlayer(
                    scenes,
                    "./Build/DummyBuild.exe",
                    BuildTarget.StandaloneWindows,
                    BuildOptions.Development
            );

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception(report.summary.ToString());
            }
        }
    }
}
