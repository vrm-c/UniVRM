using UnityEditor;

namespace UniVRM10
{
    public static class Vrm10TopMenu
    {
        private const string UserMenuPrefix = VRMVersion.MENU;
        private const string DevelopmentMenuPrefix = VRMVersion.MENU + "/Development";
        private const string ExperimentalMenuPrefix = VRMVersion.MENU + "/Experimental";

        [MenuItem(UserMenuPrefix + "/Export VRM-1.0", priority = 1)]
        private static void OpenExportDialog() => VRM10ExportDialog.Open();

        [MenuItem(ExperimentalMenuPrefix + "/Convert BVH to VRM-Animation", priority = 100)]
        private static void ConvertVrmAnimation() => VrmAnimationMenu.BvhToVrmAnimationMenu();

#if VRM_DEVELOP        
        [MenuItem(UserMenuPrefix + "/VRM1 Window", false, 2)]
        private static void OpenWindow() => VRM10Window.Open();

        [MenuItem(DevelopmentMenuPrefix + "/Generate from JsonSchema")]
        private static void Generate() => Vrm10SerializerGenerator.Run(false);

        [MenuItem(DevelopmentMenuPrefix + "/Generate from JsonSchema(debug)")]
        private static void Parse() => Vrm10SerializerGenerator.Run(true);
#endif
    }
}
