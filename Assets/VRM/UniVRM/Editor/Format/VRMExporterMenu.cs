using UnityEditor;

namespace VRM
{
    public static class VRMExporterMenu
    {
        const string CONVERT_HUMANOID_KEY = VRMVersion.MENU + "/Export humanoid";

        [MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            VRMExporterWizard.CreateWizard();
        }
    }
}
