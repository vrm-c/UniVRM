using UniGLTF;
using UnityEngine;
using System.Linq;
using UnityEditor.AssetImporters;


namespace UniVRM10
{
    [ScriptedImporter(1, "vrma")]
    public class VrmaScriptedImporter : ScriptedImporter
    {
        /// <summary>
        /// Vrm-1.0 の Asset にアイコンを付与する
        /// </summary>
        static Texture2D _AssetIcon = null;
        static Texture2D AssetIcon
        {
            get
            {
                if (_AssetIcon == null)
                {
                    // try package
                    _AssetIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.vrmc.vrm/Icons/vrm-48x48.png");
                }
                if (_AssetIcon == null)
                {
                    // try assets
                    _AssetIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/VRM10/Icons/vrm-48x48.png");
                }
                return _AssetIcon;
            }
        }

        public override void OnImportAsset(AssetImportContext context)
        {
            // 2 回目以降の Asset Import において、 Importer の設定で Extract した UnityEngine.Object が入る
            var extractedObjects = GetExternalObjectMap()
                .Where(x => x.Value != null)
                .ToDictionary(kv => new SubAssetKey(kv.Value.GetType(), kv.Key.name), kv => kv.Value);

            using (var data = new AutoGltfFileParser(assetPath).Parse())
            {
                var vrmaData = new VrmAnimationData(data);
                using (var loader = new VrmAnimationImporter(vrmaData, extractedObjects))
                {
                    var loaded = loader.Load();

                    loaded.TransferOwnership((k, o) =>
                    {
                        context.AddObjectToAsset(k.Name, o);
                    });

                    var root = loaded.Root;
                    GameObject.DestroyImmediate(loaded);

                    // var vrma = root.GetComponent<Vrm10AnimationInstance>();
                    // context.AddObjectToAsset("__boxman_mesh__", vrma.BoxMan.sharedMesh);
                    // context.AddObjectToAsset("__boxman_mesh__material__", vrma.BoxMan.sharedMaterial);

                    context.AddObjectToAsset(root.name, root, AssetIcon);
                    context.SetMainObject(root);
                }
            }
        }
    }
}