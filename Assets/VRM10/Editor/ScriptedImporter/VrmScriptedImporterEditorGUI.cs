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

        public override void OnEnable()
        {
            base.OnEnable();

            m_importer = target as VrmScriptedImporter;
            m_parser = VrmScriptedImporterImpl.Parse(m_importer.assetPath, m_importer.MigrateToVrm1);
            if (m_parser == null)
            {
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
