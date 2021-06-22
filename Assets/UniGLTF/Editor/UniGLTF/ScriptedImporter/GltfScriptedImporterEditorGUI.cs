using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using VRMShaders;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    [CustomEditor(typeof(GltfScriptedImporter))]
    public class GltfScriptedImporterEditorGUI : RemapScriptedImporterEditorBase
    {
        GltfScriptedImporter m_importer;
        GltfParser m_parser;

        RemapEditorMaterial m_materialEditor;
        RemapEditorAnimation m_animationEditor;

        public override void OnEnable()
        {
            base.OnEnable();

            m_importer = target as GltfScriptedImporter;
            m_parser = new GltfParser();
            m_parser.ParsePath(m_importer.assetPath);

            var materialGenerator = new GltfMaterialDescriptorGenerator();
            var materialKeys = m_parser.GLTF.materials.Select((_, i) => materialGenerator.Get(m_parser, i).SubAssetKey);
            var textureKeys = new GltfTextureDescriptorGenerator(m_parser).Get().GetEnumerable().Select(x => x.SubAssetKey);
            m_materialEditor = new RemapEditorMaterial(materialKeys.Concat(textureKeys), GetEditorMap, SetEditorMap);
            m_animationEditor = new RemapEditorAnimation(AnimationImporterUtil.EnumerateSubAssetKeys(m_parser.GLTF), GetEditorMap, SetEditorMap);
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
                    m_animationEditor.OnGUI(m_importer, m_parser);
                    RevertApplyRemapGUI(m_importer);
                    break;

                case Tabs.Materials:
                    m_materialEditor.OnGUI(m_importer, m_parser,
                    new GltfTextureDescriptorGenerator(m_parser),
                    assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.Textures",
                    assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.Materials");
                    RevertApplyRemapGUI(m_importer);
                    break;
            }
        }
    }
}
