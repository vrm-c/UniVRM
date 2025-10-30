using UnityEditor;

namespace UniVRM10.DevOnly
{
    public static class BuildClass
    {
        public static void BuildWebGL_VRM10Viewer()
        {
            var scenes = new string[]{
                "./Assets/VRM10_Samples/VRM10Viewer/VRM10Viewer.unity",
            };

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Build/VRM10Viewer",
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