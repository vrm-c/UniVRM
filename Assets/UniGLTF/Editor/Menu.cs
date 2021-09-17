using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    /// <summary>
    /// Menuは一か所で管理する
    /// </summary>
    public static class Menu
    {
        #region UniGLTF
        const string MENU_KEY = UniGLTFVersion.MENU + "/Export " + UniGLTFVersion.UNIGLTF_VERSION;

        [MenuItem(MENU_KEY, false, 0)]
        private static void ExportFromMenu()
        {
            var window = (GltfExportWindow)GltfExportWindow.GetWindow(typeof(GltfExportWindow));
            window.titleContent = new GUIContent("Gltf Exporter");
            window.Show();
        }

        [MenuItem(UniGLTFVersion.MENU + "/GLTF: Generate Deserializer")]
        static void GenerateDeserializer()
        {
            DeserializerGenerator.GenerateSerializer();
        }

        [MenuItem(UniGLTFVersion.MENU + "/GLTF: Generate Serializer")]
        static void GenerateSerializer()
        {
            DeserializerGenerator.GenerateSerializer();
        }
        #endregion

        #region  MeshUtility
        // [MenuItem(MeshUtility.MENU_PARENT + "BoneMeshEraser Wizard", priority = 31)]
        // static void CreateWizard()
        // {
        //     ScriptableWizard.DisplayWizard<BoneMeshEraserWizard>("BoneMeshEraser", "Erase triangles by bone", "Erase");
        // }
        const string MESH_UTILITY_DICT = "UniGLTF/Mesh Utility/";
        [MenuItem(MESH_UTILITY_DICT + "MeshProcessing Wizard", priority = 30)]
        static void MeshProcessFromMenu()
        {
            var window = (MeshUtility.MeshProcessDialog)EditorWindow.GetWindowWithRect(typeof(MeshUtility.MeshProcessDialog), new Rect(0, 0, 650, 500));
            window.titleContent = new GUIContent("Mesh Processing Window");
            window.Show();
        }

        [MenuItem(MESH_UTILITY_DICT + "MeshUtility Docs", priority = 32)]
        public static void MeshUtilityDocs()
        {
            Application.OpenURL("https://vrm.dev/en/docs/univrm/gltf/mesh_utility/");
        }

        [MenuItem("Assets/SaveAsPng", true)]
        [MenuItem("Assets/SaveAsPngLinear", true)]
        static bool IsTextureAsset()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/SaveAsPng")]
        static void SaveAsPng()
        {
            MeshUtility.EditorChangeTextureType.SaveAsPng(true);
        }

        [MenuItem("Assets/SaveAsPngLinear")]
        static void SaveAsPngLinear()
        {
            MeshUtility.EditorChangeTextureType.SaveAsPng(false);
        }
        #endregion

        [MenuItem("UniGLTF/UniJSON/Generate ConcreteCast")]
        static void GenerateConcreteCast()
        {
            UniJSON.ConcreteCast.GenerateGenericCast();
        }
    }
}
