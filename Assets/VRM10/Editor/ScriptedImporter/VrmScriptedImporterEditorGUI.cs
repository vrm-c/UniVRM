using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.IO;
using UniGLTF.MeshUtility;
using System.Linq;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    [CustomEditor(typeof(VrmScriptedImporter))]
    public class VrmScriptedImporterEditorGUI : RemapScriptedImporterEditorBase
    {
        VrmScriptedImporter m_importer;
        VrmLib.Model m_model;

        RemapEditorMaterial m_materialEditor;
        RemapEditorVrm m_vrmEditor;

        Vrm10Parser.Result m_result;

        public override void OnEnable()
        {
            base.OnEnable();

            m_importer = target as VrmScriptedImporter;
            if (!Vrm10Parser.TryParseOrMigrate(m_importer.assetPath, m_importer.MigrateToVrm1, out m_result))
            {
                // error
                return;
            }
            m_model = ModelReader.Read(m_result.Data);

            var tmp = m_importer.GetExternalObjectMap();

            var generator = new Vrm10MaterialDescriptorGenerator();
            var materialKeys = m_result.Data.GLTF.materials.Select((x, i) => generator.Get(m_result.Data, i).SubAssetKey);
            var textureKeys = new GltfTextureDescriptorGenerator(m_result.Data).Get().GetEnumerable().Select(x => x.SubAssetKey);
            m_materialEditor = new RemapEditorMaterial(materialKeys.Concat(textureKeys), GetEditorMap, SetEditorMap);
            var expressionSubAssetKeys = m_result.Vrm.Expressions.Select(x => ExpressionKey.CreateFromVrm10(x).SubAssetKey);
            m_vrmEditor = new RemapEditorVrm(new[] { VRM10Object.SubAssetKey }.Concat(expressionSubAssetKeys), GetEditorMap, SetEditorMap);
        }

        enum Tabs
        {
            Model,
            Materials,
            Vrm,
        }
        static Tabs s_currentTab;

        public override void OnInspectorGUI()
        {
            s_currentTab = TabBar.OnGUI(s_currentTab);
            GUILayout.Space(10);

            switch (s_currentTab)
            {
                case Tabs.Model:
                    {
                        switch (m_result.FileType)
                        {
                            case Vrm10FileType.Vrm1:
                                EditorGUILayout.HelpBox(m_result.Message, MessageType.Info);
                                break;

                            case Vrm10FileType.Vrm0:
                                EditorGUILayout.HelpBox(m_result.Message, m_model != null ? MessageType.Info : MessageType.Warning);
                                // migration check boxs
                                base.OnInspectorGUI();
                                break;

                            default:
                                break;
                        }
                    }
                    break;

                case Tabs.Materials:
                    if (m_result.Data != null && m_result.Vrm != null)
                    {
                        m_materialEditor.OnGUI(m_importer, m_result.Data, new Vrm10TextureDescriptorGenerator(m_result.Data),
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Textures",
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Materials");
                        RevertApplyRemapGUI(m_importer);
                    }
                    break;

                case Tabs.Vrm:
                    if (m_result.Data != null && m_result.Vrm != null)
                    {
                        m_vrmEditor.OnGUI(m_importer, m_result.Data, m_result.Vrm);
                        RevertApplyRemapGUI(m_importer);
                    }
                    break;
            }
        }
    }
}
