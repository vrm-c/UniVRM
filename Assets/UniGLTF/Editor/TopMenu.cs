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

        [MenuItem(UserGltfMenuPrefix + "/Version: " + UniGLTFVersion.UNIGLTF_VERSION, validate = true)]
        private static bool ShowVersionValidation() => false;

        [MenuItem(UserGltfMenuPrefix + "/Version: " + UniGLTFVersion.UNIGLTF_VERSION, priority = 0)]
        private static void ShowVersion() { }

        [MenuItem(UserGltfMenuPrefix + "/Export to GLB", priority = 1)]
        private static void ExportGameObjectToGltf() => TopMenuImplementation.ExportGameObjectToGltfFile();

        [MenuItem(UserGltfMenuPrefix + "/Import from GLTF (*.gltf|*.glb|*.zip)", priority = 2)]
        private static void ImportGltfFile() => TopMenuImplementation.ImportGltfFileToGameObject();

        [MenuItem(UserMeshUtilityPrefix + "/MeshProcessing Wizard", priority = 10)]
        private static void OpenMeshProcessingWindow() => MeshUtility.MeshProcessDialog.OpenWindow();

#if VRM_DEVELOP
        [MenuItem(DevelopmentMenuPrefix + "/Generate Serialization Code", priority = 20)]
        private static void GenerateSerializationCode() => TopMenuImplementation.GenerateSerializationCode();


        [MenuItem(DevelopmentMenuPrefix + "/Generate UniJSON ConcreteCast", priority = 21)]
        private static void GenerateUniJsonConcreteCastCode() => UniJSON.ConcreteCast.GenerateGenericCast();
#endif
    }
}
