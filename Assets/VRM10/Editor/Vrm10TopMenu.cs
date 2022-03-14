using UnityEditor;

namespace UniVRM10
{
    public static class Vrm10TopMenu
    {
        private const string UserMenuPrefix = VRMVersion.MENU;
        private const string DevelopmentMenuPrefix = VRMVersion.MENU + "/Development";

        const string CONVERT_HUMANOID_KEY = VRMVersion.MENU + "/Export VRM-1.0";
        [MenuItem(UserMenuPrefix + "/Export VRM-1.0", priority = 1)]
        static void OpenExportDialog() => VRM10ExportDialog.Open();

#if VRM_DEVELOP        
        [MenuItem(UserMenuPrefix + "/VRM1 Window", false, 2)]
        static void OpenWindow() => VRM10Window.Open();

        [MenuItem(DevelopmentMenuPrefix + "/Generate from JsonSchema")]
        public static void Generate() => Vrm10SerializerGenerator.Run(false);

        [MenuItem(DevelopmentMenuPrefix + "/Generate from JsonSchema(debug)")]
        public static void Parse() => Vrm10SerializerGenerator.Run(true);
#endif
    }
}
