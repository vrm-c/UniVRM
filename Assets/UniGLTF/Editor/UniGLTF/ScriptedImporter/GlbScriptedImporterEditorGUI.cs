using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace UniGLTF
{
    [CustomEditor(typeof(GlbScriptedImporter))]
    public class GlbScriptedImporterEditorGUI : ScriptedImporterEditor
    {
        GlbScriptedImporter m_importer;
        GltfParser m_parser;

        public override void OnEnable()
        {
            m_importer = target as GlbScriptedImporter;
            m_parser = new GltfParser();
            m_parser.ParsePath(m_importer.assetPath);
        }

        enum Tabs
        {
            Model,
            Animation,
            Materials,
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

                case Tabs.Animation:
                    EditorAnimation.OnGUIAnimation(m_parser);
                    break;

                case Tabs.Materials:
                    EditorMaterial.OnGUIMaterial(m_importer, m_parser);
                    break;
            }
        }
    }
}
