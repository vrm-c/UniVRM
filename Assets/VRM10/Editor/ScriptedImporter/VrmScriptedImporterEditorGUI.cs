using UnityEditor;
using UnityEngine;
using UniGLTF;
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

        string m_message;

        public override void OnEnable()
        {
            base.OnEnable();

            m_importer = target as VrmScriptedImporter;
            m_message = VrmScriptedImporterImpl.TryParseOrMigrate(m_importer.assetPath, m_importer.MigrateToVrm1, out m_parser);
            if (string.IsNullOrEmpty(m_message))
            {
                // ok
                return;
            }
            if (!UniGLTF.Extensions.VRMC_vrm.GltfDeserializer.TryGet(m_parser.GLTF.extensions, out m_vrm))
            {
                return;
            }
            m_model = VrmLoader.CreateVrmModel(m_parser);
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

            s_currentTab = MeshUtility.TabBar.OnGUI(s_currentTab);
            GUILayout.Space(10);

            switch (s_currentTab)
            {
                case Tabs.Model:
                    base.OnInspectorGUI();
                    break;

                case Tabs.Materials:
                    if (m_parser != null)
                    {
                        EditorMaterial.OnGUI(m_importer, m_parser, Vrm10MToonMaterialImporter.EnumerateAllTexturesDistinct);
                    }
                    break;

                case Tabs.Vrm:
                    if (m_parser != null)
                    {
                        EditorVrm.OnGUI(m_importer, m_parser, m_vrm);
                    }
                    break;
            }
        }
    }
}
