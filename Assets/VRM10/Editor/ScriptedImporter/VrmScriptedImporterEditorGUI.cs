using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.IO;
using UniGLTF.MeshUtility;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniVRM10
{
    [CustomEditor(typeof(VrmScriptedImporter))]
    public class VrmScriptedImporterEditorGUI : ScriptedImporterEditor
    {
        VrmScriptedImporter m_importer;
        GltfParser m_parser;
        VrmLib.Model m_model;
        UniGLTF.Extensions.VRMC_vrm.VRMC_vrm m_vrm;

        RemapEditorMaterial m_materialEditor = new RemapEditorMaterial();
        RemapEditorVrm m_vrmEditor = new RemapEditorVrm();

        string m_message;

        public override void OnEnable()
        {
            base.OnEnable();

            m_importer = target as VrmScriptedImporter;
            m_parser = default;
            m_message = default;
            if (!Vrm10Parser.TryParseOrMigrate(m_importer.assetPath, m_importer.MigrateToVrm1, out Vrm10Parser.Result result, out m_message))
            {
                // error
                return;
            }
            m_vrm = result.Vrm;
            m_parser = result.Parser;
            m_model = ModelReader.Read(result.Parser);
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
            if (!string.IsNullOrEmpty(m_message))
            {
                EditorGUILayout.HelpBox(m_message, MessageType.Error);
            }

            s_currentTab = TabBar.OnGUI(s_currentTab);
            GUILayout.Space(10);

            switch (s_currentTab)
            {
                case Tabs.Model:
                    base.OnInspectorGUI();
                    break;

                case Tabs.Materials:
                    if (m_parser != null && m_vrm != null)
                    {
                        m_materialEditor.OnGUI(m_importer, m_parser, new Vrm10TextureDescriptorGenerator(m_parser),
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Textures",
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Materials");
                    }
                    break;

                case Tabs.Vrm:
                    if (m_parser != null && m_vrm != null)
                    {
                        m_vrmEditor.OnGUI(m_importer, m_parser, m_vrm);
                    }
                    break;
            }
        }
    }
}
