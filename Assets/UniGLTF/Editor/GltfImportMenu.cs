using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniGLTF
{
    public static class GltfImportMenu
    {
        public static void ImportGltfFileToGameObject()
        {
            if(!UniGltfEditorDialog.TryOpenFilePanel("", out var path))
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
    }
}
