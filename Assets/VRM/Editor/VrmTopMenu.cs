using UniGLTF;
using UnityEditor;
using VRM.DevOnly.PackageExporter;

namespace VRM
{
    public static class VrmTopMenu
    {
        private const string UserMenuPrefix = PackageVersion.MENU;
        private const string DevelopmentMenuPrefix = PackageVersion.MENU + "/Development";


        [MenuItem(UserMenuPrefix + "/" + PackageVersion.MENU_NAME, true, 0)]
        private static bool ShowVersionValidation() => false;
        [MenuItem(UserMenuPrefix + "/" + PackageVersion.MENU_NAME, false, 0)]
        private static void ShowVersion() { }


        [MenuItem(UserMenuPrefix + "/" + VRMExporterWizard.MENU_NAME, false, 1)]
        private static void ExportToVrmFile() => VRMExporterWizard.OpenExportMenu();


        [MenuItem(UserMenuPrefix + "/" + VRMImporterMenu.MENU_NAME, false, 2)]
        private static void ImportFromVrmFile() => VRMImporterMenu.OpenImportMenu();


        [MenuItem(UserMenuPrefix + "/" + VrmMeshIntegratorWizard.MENU_NAME, false, 51)]
        private static void OpenMeshIntegratorWizard() => VrmMeshIntegratorWizard.OpenWindow();


        [MenuItem(UserMenuPrefix + "/" + VRMHumanoidNormalizerMenu.MENU_NAME, true, 52)]
        private static bool FreezeTPoseValidation() => VRMHumanoidNormalizerMenu.NormalizeValidation();
        [MenuItem(UserMenuPrefix + "/" + VRMHumanoidNormalizerMenu.MENU_NAME, false, 52)]
        private static void FreezeTPose() => VRMHumanoidNormalizerMenu.Normalize(bakeCurrentBlendShape: true);


        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.SAVE_MENU_NAME, true, 53)]
        private static bool SaveSpringBoneToJsonValidation() => VRMSpringBoneUtilityEditor.SaveSpringBoneToJsonValidation();
        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.SAVE_MENU_NAME, false, 53)]
        private static void SaveSpringBoneToJson() => VRMSpringBoneUtilityEditor.SaveSpringBoneToJson();


        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.LOAD_MENU_NAME, true, 54)]
        private static bool LoadSpringBoneFromJsonValidation() => VRMSpringBoneUtilityEditor.LoadSpringBoneFromJsonValidation();
        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.LOAD_MENU_NAME, false, 54)]
        private static void LoadSpringBoneFromJson() => VRMSpringBoneUtilityEditor.LoadSpringBoneFromJson();

#if VRM_DEVELOP
        [MenuItem(DevelopmentMenuPrefix + "/Generate Serialization Code", false, 91)]
        private static void GenerateSerializer() => VRMAOTCodeGenerator.GenerateCode();

        [MenuItem(DevelopmentMenuPrefix + "/Version Dialog", false, 92)]
        private static void ShowVersionDialog() => VRMVersionMenu.ShowVersionDialog();

        [MenuItem(DevelopmentMenuPrefix + "/Build dummy for CI", false, 93)]
        private static void BuildDummyForCi() => BuildClass.Build();

        [MenuItem(DevelopmentMenuPrefix + "/Create UnityPackage", false, 94)]
        private static void CreateUnityPackage() => VRMExportUnityPackage.CreateUnityPackageWithoutBuild();
#endif
    }
}