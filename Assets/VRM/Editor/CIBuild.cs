using UnityEditor;

namespace VRM
{
    public class BuildClass
    {
        /// <summary>
        /// dummy build for CI
        /// </summary>
#if VRM_DEVELOP
        [MenuItem(VRMVersion.MENU + "/dummy_build")]
#endif
        public static void Build()
        {
            var scenes = new string[]{
                "./Assets/VRM/Samples/SimpleViewer/SimpleViewer.unity",
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
