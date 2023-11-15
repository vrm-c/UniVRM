using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;

namespace UniGLTF
{
    /// <summary>
    /// 機能をどのようにトップメニューに表示するかどうかだけを記述する.
    /// </summary>
    public static class TopMenu
    {
        private const string UserGltfMenuPrefix = UniGLTFVersion.MENU;
        private const string DevelopmentMenuPrefix = UniGLTFVersion.MENU + "/Development";


        [MenuItem(UserGltfMenuPrefix + "/Version: " + UniGLTFVersion.UNIGLTF_VERSION, validate = true)]
        private static bool ShowVersionValidation() => false;
        [MenuItem(UserGltfMenuPrefix + "/Version: " + UniGLTFVersion.UNIGLTF_VERSION, priority = 0)]
        private static void ShowVersion() { }


        [MenuItem(UserGltfMenuPrefix + "/" + GltfExportWindow.MENU_NAME, priority = 1)]
        private static void ExportGameObjectToGltf() => GltfExportWindow.ExportGameObjectToGltfFile();


        [MenuItem(UserGltfMenuPrefix + "/" + GltfImportMenu.MENU_NAME, priority = 2)]
        private static void ImportGltfFile() => GltfImportMenu.ImportGltfFileToGameObject();


        [MenuItem(UserGltfMenuPrefix + "/" + MeshUtility.MeshUtilityDialog.MENU_NAME, priority = 10)]
        private static void OpenMeshProcessingWindow() => MeshUtility.MeshUtilityDialog.OpenWindow();

#if VRM_DEVELOP
        [MenuItem(DevelopmentMenuPrefix + "/Generate Serialization Code", priority = 20)]
        private static void GenerateSerializationCode()
        {
            SerializerGenerator.GenerateSerializer();
            DeserializerGenerator.GenerateSerializer();
        }

        [MenuItem(DevelopmentMenuPrefix + "/Generate UniJSON ConcreteCast", priority = 21)]
        private static void GenerateUniJsonConcreteCastCode() => UniJSON.ConcreteCast.GenerateGenericCast();
#endif
    }
}
