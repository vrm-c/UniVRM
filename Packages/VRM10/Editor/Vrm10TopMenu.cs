using UniGLTF.MeshUtility;
using UnityEditor;

namespace UniVRM10
{
    public static class Vrm10TopMenu
    {
        private const string UserMenuPrefix = VRM10SpecVersion.MENU;
        private const string DevelopmentMenuPrefix = VRM10SpecVersion.MENU + "/Development";
        private const string ExperimentalMenuPrefix = VRM10SpecVersion.MENU + "/Experimental";


        [MenuItem(UserMenuPrefix + "/" + VRM10ExportDialog.MENU_NAME, priority = 1)]
        private static void OpenExportDialog() => VRM10ExportDialog.Open();

        [MenuItem(UserMenuPrefix + "/" + Vrm10MeshUtilityDialog.MENU_NAME, priority = 21)]
        private static void OpenMeshUtility() => Vrm10MeshUtilityDialog.OpenWindow();


        [MenuItem(ExperimentalMenuPrefix + "/" + VrmAnimationMenu.MENU_NAME, priority = 22)]
        private static void ConvertVrmAnimation() => VrmAnimationMenu.BvhToVrmAnimationMenu();

#if VRM_DEVELOP        
        [MenuItem(DevelopmentMenuPrefix + "/Generate from JsonSchema", false, 100)]
        private static void Generate() => Vrm10SerializerGenerator.Run(false);

        [MenuItem(DevelopmentMenuPrefix + "/Generate from JsonSchema(debug)", false, 101)]
        private static void Parse() => Vrm10SerializerGenerator.Run(true);

        [MenuItem(DevelopmentMenuPrefix + "/Build WebGL VRM10Viewer for CI", false, 201)]
        private static void BuildWebGLForCi10() => DevOnly.BuildClass.BuildWebGL_VRM10Viewer();
#endif
    }
}
