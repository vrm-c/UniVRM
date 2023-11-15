using UniGLTF;
using UnityEditor;
using VRM.DevOnly.PackageExporter;

namespace VRM
{
    public static class VrmTopMenu
    {
        private const string UserMenuPrefix = PackageVersion.MENU;
        private const string DevelopmentMenuPrefix = PackageVersion.MENU + "/Development";


        [MenuItem(UserMenuPrefix + "/" + PackageVersion.MENU_NAME, validate = true)]
        private static bool ShowVersionValidation() => false;
        [MenuItem(UserMenuPrefix + "/" + PackageVersion.MENU_NAME, priority = 0)]
        private static void ShowVersion() { }


        [MenuItem(UserMenuPrefix + "/" + VRMExporterWizard.MENU_NAME, priority = 1)]
        private static void ExportToVrmFile() => VRMExporterWizard.OpenExportMenu();


        [MenuItem(UserMenuPrefix + "/" + VRMImporterMenu.MENU_NAME, priority = 2)]
        private static void ImportFromVrmFile() => VRMImporterMenu.OpenImportMenu();


        [MenuItem(UserMenuPrefix + "/" + VRMHumanoidNormalizerMenu.MENU_NAME, validate = true)]
        private static bool FreezeTPoseValidation() => VRMHumanoidNormalizerMenu.NormalizeValidation();
        [MenuItem(UserMenuPrefix + "/" + VRMHumanoidNormalizerMenu.MENU_NAME, priority = 20)]
        private static void FreezeTPose() => VRMHumanoidNormalizerMenu.Normalize();


        [MenuItem(UserMenuPrefix + "/" + VrmMeshIntegratorWizard.MENU_NAME, priority = 21)]
        private static void OpenMeshIntegratorWizard() => VrmMeshIntegratorWizard.CreateWizard();


        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.SAVE_MENU_NAME, validate = true)]
        private static bool SaveSpringBoneToJsonValidation() => VRMSpringBoneUtilityEditor.SaveSpringBoneToJsonValidation();
        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.SAVE_MENU_NAME, priority = 22)]
        private static void SaveSpringBoneToJson() => VRMSpringBoneUtilityEditor.SaveSpringBoneToJson();


        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.LOAD_MENU_NAME, validate = true)]
        private static bool LoadSpringBoneFromJsonValidation() => VRMSpringBoneUtilityEditor.LoadSpringBoneFromJsonValidation();
        [MenuItem(UserMenuPrefix + "/" + VRMSpringBoneUtilityEditor.LOAD_MENU_NAME, priority = 23)]


#if VRM_DEVELOP
        [MenuItem(DevelopmentMenuPrefix + "/Generate Serialization Code", priority = 30)]
        private static void GenerateSerializer() => VRMAOTCodeGenerator.GenerateCode();

        [MenuItem(DevelopmentMenuPrefix + "/Version Dialog", priority = 32)]
        private static void ShowVersionDialog() => VRMVersionMenu.ShowVersionDialog();

        [MenuItem(DevelopmentMenuPrefix + "/Build dummy for CI", priority = 33)]
        private static void BuildDummyForCi() => BuildClass.Build();

        [MenuItem(DevelopmentMenuPrefix + "/Create UnityPackage", priority = 34)]
        private static void CreateUnityPackage() => VRMExportUnityPackage.CreateUnityPackageWithoutBuild();
#endif
    }
}