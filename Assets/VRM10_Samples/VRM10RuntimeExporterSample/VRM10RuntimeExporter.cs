using System.IO;
using UniGLTF;
using UnityEngine;
using VRMShaders;

namespace UniVRM10.RuntimeExporterSample
{
    public class VRM10RuntimeExporter : MonoBehaviour
    {
        [SerializeField]
        VRM10ObjectMeta m_meta = new VRM10ObjectMeta();

        GameObject m_model;

        void OnGUI()
        {
            if (GUILayout.Button("Load"))
            {
                Load();
            }

            GUI.enabled = m_model != null;

            // if (GUILayout.Button("Add custom blend shape"))
            // {
            //     AddBlendShapeClip(m_model);
            // }

            if (GUILayout.Button("Export"))
            {
                Export(m_model);
            }
        }

        async void Load()
        {
#if UNITY_STANDALONE_WIN
            var path = VRM10FileDialogForWindows.FileDialog("open VRM", ".vrm");
#elif UNITY_EDITOR
            var path = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
#else
            var path = Application.dataPath + "/default.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var vrm10 = await Vrm10.LoadPathAsync(path);
            var loaded = vrm10.GetComponent<UniGLTF.RuntimeGltfInstance>();
            loaded.ShowMeshes();
            loaded.EnableUpdateWhenOffscreen();

            if (m_model != null)
            {
                GameObject.Destroy(m_model.gameObject);
            }

            m_model = loaded.gameObject;
        }

        // static void AddBlendShapeClip(GameObject go)
        // {
        //     // get or create blendshape proxy
        //     var proxy = go.GetComponent<VRMBlendShapeProxy>();
        //     if (proxy == null)
        //     {
        //         proxy = go.AddComponent<VRMBlendShapeProxy>();
        //     }
        //
        //     // get or create blendshapeavatar
        //     var avatar = proxy.BlendShapeAvatar;
        //     if (avatar == null)
        //     {
        //         avatar = ScriptableObject.CreateInstance<BlendShapeAvatar>();
        //         proxy.BlendShapeAvatar = avatar;
        //     }
        //
        //     // add blendshape clip to avatar.Clips
        //     var clip = ScriptableObject.CreateInstance<BlendShapeClip>();
        //     var name = $"custom#{avatar.Clips.Count}";
        //     Debug.Log($"Add {name}");
        //     // unity asset name
        //     clip.name = name;
        //     // vrm export name
        //     clip.BlendShapeName = name;
        //     clip.Preset = BlendShapePreset.Unknown;
        //
        //     clip.IsBinary = false;
        //     clip.Values = new BlendShapeBinding[]
        //     {
        //         new BlendShapeBinding
        //         {
        //             RelativePath = "mesh/face", // target Renderer relative path from root 
        //             Index = 0, // BlendShapeIndex in SkinnedMeshRenderer
        //             Weight = 75f // BlendShape weight, range is [0-100]
        //         },
        //     };
        //     clip.MaterialValues = new MaterialValueBinding[]
        //     {
        //         new MaterialValueBinding
        //         {
        //             MaterialName = "Alicia_body", // target_material_name
        //             ValueName = "_Color", // target_material_property_name,
        //             BaseValue = new Vector4(1, 1, 1, 1), // Target value when the Weight value of BlendShapeClip is 0
        //             TargetValue = new Vector4(0, 0, 0, 1), // Target value when the Weight value of BlendShapeClip is 1
        //         },
        //     };
        //     avatar.Clips.Add(clip);
        //
        //     // done
        // }


        void Export(GameObject model)
        {
            //#if UNITY_STANDALONE_WIN
#if false
        var path = FileDialogForWindows.SaveDialog("save VRM", Application.dataPath + "/export.vrm");
#else
            var path = Application.dataPath + "/../export.vrm";
#endif
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var bytes = ExportSimple(model, m_meta);

            File.WriteAllBytes(path, bytes);
            Debug.LogFormat("export to {0}", path);
        }

        static byte[] ExportSimple(GameObject root, VRM10ObjectMeta meta)
        {
            using (var arrayManager = new NativeArrayManager())
            {
                var converter = new UniVRM10.ModelExporter();
                var model = converter.Export(arrayManager, root);

                // 右手系に変換
                Debug.Log($"convert to right handed coordinate...");
                model.ConvertCoordinate(VrmLib.Coordinates.Vrm1, ignoreVrm: false);

                // export vrm-1.0
                var exporter = new UniVRM10.Vrm10Exporter(new RuntimeTextureSerializer(), new GltfExportSettings
                {

                });
                exporter.Export(root, model, converter, new VrmLib.ExportArgs
                {
                }, meta);

                return exporter.Storage.ToGlbBytes();
            }

            // var vrm = VRMExporter.Export(new UniGLTF.GltfExportSettings(), model, new RuntimeTextureSerializer());
            // var bytes = vrm.ToGlbBytes();
            // return bytes;
            throw new System.NotImplementedException();
        }

        void OnExported(UniGLTF.glTF vrm)
        {
            Debug.LogFormat("exported");
        }
    }
}
