using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// 機能をどのようにトップメニューに表示するかどうかだけを記述する.
    /// </summary>
    public static class TopMenu
    {
        private const string UserGltfMenuPrefix = UniGLTFVersion.MENU;
        private const string UserMeshUtilityPrefix = UniGLTFVersion.MENU + "/Mesh Utility";
        private const string DevelopmentMenuPrefix = UniGLTFVersion.MENU + "/Development";
        private const int UserGltfMenuPriority = 0;
        private const int UserMeshUtilityMenuPriority = 10;
        private const int DevelopmentMenuPriority = 30;

        #region USER
        [MenuItem(
            UserGltfMenuPrefix + "/Version: " + UniGLTFVersion.UNIGLTF_VERSION,
            validate = true)]
        private static bool ShowVersionValidation() => false;

        [MenuItem(
            UserGltfMenuPrefix + "/Version: " + UniGLTFVersion.UNIGLTF_VERSION,
            priority = UserGltfMenuPriority + 0)]
        private static void ShowVersion()
        {
        }

        [MenuItem(
            UserGltfMenuPrefix + "/Export to GLB",
            priority = UserGltfMenuPriority + 1)]
        private static void ExportGameObjectToGltf() => TopMenuImplementation.ExportGameObjectToGltfFile();

        [MenuItem(
            UserGltfMenuPrefix + "/Import from GLTF (*.gltf|*.glb|*.zip)",
            priority = UserGltfMenuPriority + 2)]
        private static void ImportGltfFile() => TopMenuImplementation.ImportGltfFileToGameObject();
        #endregion

        #region MESH_UTILITY

        [MenuItem(
            UserMeshUtilityPrefix + "/MeshProcessing Wizard",
            priority = UserMeshUtilityMenuPriority + 0)]
        private static void OpenMeshProcessingWindow() => TopMenuImplementation.OpenMeshProcessingWindow();

        [MenuItem(
            UserMeshUtilityPrefix + "/Open Documents",
            priority = UserMeshUtilityMenuPriority + 1)]
        private static void MeshUtilityDocs() => Application.OpenURL("https://vrm.dev/en/docs/univrm/gltf/mesh_utility/");
        #endregion

        #region DEVELOPMENT
        [MenuItem(
            DevelopmentMenuPrefix + "/Generate Serialization Code",
            priority = DevelopmentMenuPriority + 1)]
        private static void GenerateSerializationCode() => TopMenuImplementation.GenerateSerializationCode();


        [MenuItem(
            DevelopmentMenuPrefix + "/Generate UniJSON ConcreteCast",
            priority = DevelopmentMenuPriority + 2)]
        private static void GenerateUniJsonConcreteCastCode() => UniJSON.ConcreteCast.GenerateGenericCast();
        #endregion
    }
}
