using System.IO;
using System.Linq;
using UniGLTF;
using UnityEditor;


namespace VRM
{
    public class vrmAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                var ext = Path.GetExtension(path).ToLower();
                if (ext == ".vrm")
                {
                    var context = new VRMImporterContext(path);

                    context.ParseVrm(File.ReadAllBytes(context.Path));

                    //
                    // https://answers.unity.com/questions/647615/how-to-update-import-settings-for-newly-created-as.html
                    //
                    for (int i = 0; i < context.GLTF.textures.Count; ++i)
                    {
                        var x = context.GLTF.textures[i];
                        var image = context.GLTF.images[x.source];
                        if (string.IsNullOrEmpty(image.uri))
                        {
                            // glb buffer
                            var folder = context.GetAssetFolder(".Textures").AssetPathToFullPath();
                            if (!Directory.Exists(folder))
                            {
                                UnityEditor.AssetDatabase.CreateFolder(context.GLTF.baseDir, Path.GetFileNameWithoutExtension(context.Path) + ".Textures");
                                //Directory.CreateDirectory(folder);
                            }

                            var textureName = !string.IsNullOrEmpty(image.name) ? image.name: string.Format("buffer#{0:00}", i);
                            var png = Path.Combine(folder, textureName + ".png");
                            var byteSegment = context.GLTF.GetViewBytes(image.bufferView);
                            File.WriteAllBytes(png, byteSegment.ToArray());
                            var assetPath = png.ToUnityRelativePath();
                            //Debug.LogFormat("import asset {0}", assetPath);
                            UnityEditor.AssetDatabase.ImportAsset(assetPath, UnityEditor.ImportAssetOptions.ForceUpdate);
                            UnityEditor.AssetDatabase.Refresh();
                            image.uri = assetPath.Substring(context.GLTF.baseDir.Length + 1);
                        }
                    }

                    EditorApplication.delayCall += () =>
                    {
                        // delay and can import png texture
                        VRMImporter.LoadFromBytes(context);
                        context.SaveAsAsset();
                        context.Destroy(false);
                    };
                }
            }
        }
    }
}
