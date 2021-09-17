using System.IO;
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
        [MenuItem(UniGLTFVersion.MENU + "/Import(gltf, glb, zip)", priority = 1)]
        public static void ImportMenu()
        {
            var path = EditorUtility.OpenFilePanel("open glb", "", "gltf,glb,zip");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Application.isPlaying)
            {
                //
                // load into scene
                //
                var data = new AutoGltfFileParser(path).Parse();
                using (var context = new ImporterContext(data))
                {
                    var loaded = context.Load();
                    loaded.ShowMeshes();
                    Selection.activeGameObject = loaded.gameObject;
                }
                return;
            }

            //
            // save as asset
            //
            if (path.StartsWithUnityAssetPath())
            {
                Debug.LogWarningFormat("disallow import from folder under the Assets");
                return;
            }

            var ext = Path.GetExtension(path).ToLower();
            var assetPath = EditorUtility.SaveFilePanel("save prefab", "Assets", Path.GetFileNameWithoutExtension(path), ext.Substring(1));
            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            // copy
            var bytes = File.ReadAllBytes(path);
            File.WriteAllBytes(assetPath, bytes);
            if (ext == ".gltf")
            {
                // copy associated files
                var src_dir = Path.GetDirectoryName(path);
                var dst_dir = Path.GetDirectoryName(assetPath);
                var data = new GltfFileWithResourceFilesParser(path, bytes).Parse();
                foreach (var buffer in data.GLTF.buffers)
                {
                    if (!string.IsNullOrEmpty(buffer.uri))
                    {
                        var src_path = Path.Combine(src_dir, buffer.uri);
                        var src_bytes = File.ReadAllBytes(src_path);
                        var dst_path = Path.Combine(dst_dir, buffer.uri);
                        File.WriteAllBytes(dst_path, src_bytes);
                        UnityPath.FromFullpath(dst_path).ImportAsset();
                    }
                }
                foreach (var buffer in data.GLTF.images)
                {
                    if (!string.IsNullOrEmpty(buffer.uri))
                    {
                        var src_path = Path.Combine(src_dir, buffer.uri);
                        var src_bytes = File.ReadAllBytes(src_path);
                        var dst_path = Path.Combine(dst_dir, buffer.uri);
                        File.WriteAllBytes(dst_path, src_bytes);
                        UnityPath.FromFullpath(dst_path).ImportAsset();
                    }
                }
            }

            // import as asset
            var unitypath = UnityPath.FromFullpath(assetPath);
            unitypath.ImportAsset();
            var asset = unitypath.LoadAsset<GameObject>();
            Selection.activeObject = asset;
        }

        [MenuItem(UniGLTFVersion.MENU + "/Export " + UniGLTFVersion.UNIGLTF_VERSION, false, 0)]
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
