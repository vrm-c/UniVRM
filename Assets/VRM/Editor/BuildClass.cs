using UnityEditor;

namespace VRM.DevOnly
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
                    "./Build/DummyBuild/DummyBuild.exe",
                    BuildTarget.StandaloneWindows,
                    BuildOptions.Development
            );

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception(report.summary.ToString());
            }
        }

        public static void SwitchBuiltinPipeline()
        {
            UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset = null;
        }

        public static void BuildWebGL_SimpleViewer()
        {
            var scenes = new string[]{
                "./Assets/VRM_Samples/SimpleViewer/SimpleViewer.unity",
            };

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Build/SimpleViewer",
                target = BuildTarget.WebGL,
            }
            );

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception(report.summary.ToString());
            }
        }
    }
}
