using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace UniGLTF
{
    [CustomEditor(typeof(GltfScriptedImporter))]
    public class GltfScriptedImporterEditor : RemapScriptedImporterEditorBase
    {
        GltfData m_data;

        RemapEditorMaterial m_materialEditor;
        RemapEditorAnimation m_animationEditor;

        public override void OnEnable()
        {
            base.OnEnable();

            m_importer = target as GltfScriptedImporter;
            if (m_data != null)
            {
                m_data.Dispose();
            }
            m_data = new AutoGltfFileParser(m_importer.assetPath).Parse();

            var materialGenerator = new BuiltInGltfMaterialDescriptorGenerator();
            var materialKeys = m_data.GLTF.materials.Select((_, i) => materialGenerator.Get(m_data, i).SubAssetKey);
            var textureKeys = new GltfTextureDescriptorGenerator(m_data).Get().GetEnumerable().Select(x => x.SubAssetKey);
            m_materialEditor = new RemapEditorMaterial(materialKeys.Concat(textureKeys), GetEditorMap, SetEditorMap);
            m_animationEditor = new RemapEditorAnimation(AnimationImporterUtil.EnumerateSubAssetKeys(m_data.GLTF), GetEditorMap, SetEditorMap);
        }

        public override void OnDisable()
        {
            m_data.Dispose();
            m_data = null;

            base.OnDisable();
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
                    m_animationEditor.OnGUI(m_importer, m_data);
                    ApplyRevertGUI();
                    break;

                case Tabs.Materials:
                    m_materialEditor.OnGUI(m_importer, m_data,
                    new GltfTextureDescriptorGenerator(m_data),
                    assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.Textures",
                    assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.Materials");
                    ApplyRevertGUI();
                    break;
            }
        }
    }
}
