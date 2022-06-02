using UniGLTF;
using UnityEditor;
using VRM.DevOnly.PackageExporter;

namespace VRM
{
    public static class VrmTopMenu
    {
        private const string UserMenuPrefix = VRMVersion.MENU;
        private const string DevelopmentMenuPrefix = VRMVersion.MENU + "/Development";

        [MenuItem(UserMenuPrefix + "/Version: " + VRMVersion.VRM_VERSION, validate = true)]
        private static bool ShowVersionValidation() => false;

        [MenuItem(UserMenuPrefix + "/Version: " + VRMVersion.VRM_VERSION, priority = 0)]
        private static void ShowVersion() { }

        [MenuItem(UserMenuPrefix + "/Export to VRM 0.x", priority = 1)]
        private static void ExportToVrmFile() => VRMExporterWizard.OpenExportMenu();

        [MenuItem(UserMenuPrefix + "/Import from VRM 0.x", priority = 2)]
        private static void ImportFromVrmFile() => VRMImporterMenu.OpenImportMenu();

        [MenuItem(UserMenuPrefix + "/Freeze T-Pose", validate = true)]
        private static bool FreezeTPoseValidation() => VRMHumanoidNormalizerMenu.NormalizeValidation();

        [MenuItem(UserMenuPrefix + "/Freeze T-Pose", priority = 20)]
        private static void FreezeTPose() => VRMHumanoidNormalizerMenu.Normalize();

        [MenuItem(UserMenuPrefix + "/MeshIntegratorWizard", priority = 21)]
        private static void OpenMeshIntegratorWizard() => VrmMeshIntegratorWizard.CreateWizard();

        [MenuItem(UserMenuPrefix + "/Save SpringBone to JSON", validate = true)]
        private static bool SaveSpringBoneToJsonValidation() => VRMSpringBoneUtilityEditor.SaveSpringBoneToJsonValidation();

        [MenuItem(UserMenuPrefix + "/Save SpringBone to JSON", priority = 22)]
        private static void SaveSpringBoneToJson() => VRMSpringBoneUtilityEditor.SaveSpringBoneToJson();

        [MenuItem(UserMenuPrefix + "/Load SpringBone from JSON", validate = true)]
        private static bool LoadSpringBoneFromJsonValidation() => VRMSpringBoneUtilityEditor.LoadSpringBoneFromJsonValidation();

        [MenuItem(UserMenuPrefix + "/Load SpringBone from JSON", priority = 23)]
        private static void LoadSpringBoneFromJson() => VRMSpringBoneUtilityEditor.LoadSpringBoneFromJson();


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