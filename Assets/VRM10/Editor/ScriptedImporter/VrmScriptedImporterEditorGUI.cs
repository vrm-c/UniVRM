using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.IO;
using UniGLTF.MeshUtility;
using System.Linq;
using System.Collections.Generic;
using VRMShaders;
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
        VrmLib.Model m_model;

        RemapEditorMaterial m_materialEditor;
        RemapEditorVrm m_vrmEditor;

        Vrm10Data m_result;
        MigrationData m_migration;

        IEnumerable<SubAssetKey> EnumerateExpressinKeys(UniGLTF.Extensions.VRMC_vrm.Expressions expressions)
        {
            if (expressions == null)
            {
                yield break;
            }

            if (expressions.Preset?.Happy != null) yield return ExpressionKey.Happy.SubAssetKey;
            if (expressions.Preset?.Angry != null) yield return ExpressionKey.Angry.SubAssetKey;
            if (expressions.Preset?.Sad != null) yield return ExpressionKey.Sad.SubAssetKey;
            if (expressions.Preset?.Relaxed != null) yield return ExpressionKey.Relaxed.SubAssetKey;
            if (expressions.Preset?.Surprised != null) yield return ExpressionKey.Surprised.SubAssetKey;
            if (expressions.Preset?.Aa != null) yield return ExpressionKey.Aa.SubAssetKey;
            if (expressions.Preset?.Ih != null) yield return ExpressionKey.Ih.SubAssetKey;
            if (expressions.Preset?.Ou != null) yield return ExpressionKey.Ou.SubAssetKey;
            if (expressions.Preset?.Ee != null) yield return ExpressionKey.Ee.SubAssetKey;
            if (expressions.Preset?.Oh != null) yield return ExpressionKey.Oh.SubAssetKey;
            if (expressions.Preset?.Blink != null) yield return ExpressionKey.Blink.SubAssetKey;
            if (expressions.Preset?.BlinkLeft != null) yield return ExpressionKey.BlinkLeft.SubAssetKey;
            if (expressions.Preset?.BlinkRight != null) yield return ExpressionKey.BlinkRight.SubAssetKey;
            if (expressions.Preset?.LookUp != null) yield return ExpressionKey.LookUp.SubAssetKey;
            if (expressions.Preset?.LookDown != null) yield return ExpressionKey.LookDown.SubAssetKey;
            if (expressions.Preset?.LookLeft != null) yield return ExpressionKey.LookLeft.SubAssetKey;
            if (expressions.Preset?.LookRight != null) yield return ExpressionKey.LookRight.SubAssetKey;
            if (expressions.Preset?.Neutral != null) yield return ExpressionKey.Neutral.SubAssetKey;

            if (expressions.Custom != null)
            {
                foreach (var kv in expressions.Custom)
                {
                    yield return ExpressionKey.CreateCustom(kv.Key).SubAssetKey;
                }
            }
        }

        void OnData()
        {
            if (m_result == null)
            {
                // error
                return;
            }
            m_model = ModelReader.Read(m_result.Data);

            var tmp = m_importer.GetExternalObjectMap();

            var generator = new BuiltInVrm10MaterialDescriptorGenerator();
            var materialKeys = m_result.Data.GLTF.materials.Select((x, i) => generator.Get(m_result.Data, i).SubAssetKey);
            var textureKeys = new Vrm10TextureDescriptorGenerator(m_result.Data).Get().GetEnumerable().Select(x => x.SubAssetKey);
            m_materialEditor = new RemapEditorMaterial(materialKeys.Concat(textureKeys), GetEditorMap, SetEditorMap);
            m_vrmEditor = new RemapEditorVrm(new[] { VRM10Object.SubAssetKey }.Concat(EnumerateExpressinKeys(m_result.VrmExtension.Expressions)), GetEditorMap, SetEditorMap);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            var importer = target as VrmScriptedImporter;
            m_importer = importer;
            using (var data = new GlbFileParser(m_importer.assetPath).Parse())
            {
                m_result = Vrm10Data.Parse(data);
                if (m_result != null)
                {
                    OnData();
                }
                else
                {
                    using (var migrated = Vrm10Data.Migrate(data, out m_result, out m_migration))
                    {
                        if (m_result != null)
                        {
                            OnData();
                        }
                    }
                }
            }
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
                        if (m_migration == null)
                        {
                            {
                                serializedObject.Update();
                                // normalize
                                EditorGUILayout.Space();
                                EditorGUILayout.HelpBox("Create normalized prefab", MessageType.Info);
                                serializedObject.ApplyModifiedProperties();
                            }

                            ApplyRevertGUI();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(m_migration.Message, m_model != null ? MessageType.Info : MessageType.Warning);

                            if (VRMShaders.Symbols.VRM_DEVELOP)
                            {
                                if (GUILayout.Button("debug export"))
                                {
                                    File.WriteAllBytes("tmp.vrm", m_migration.MigratedBytes);
                                }
                            }

                            {
                                serializedObject.Update();
                                // migration
                                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(VrmScriptedImporter.MigrateToVrm1)));
                                serializedObject.ApplyModifiedProperties();
                            }

                            ApplyRevertGUI();
                        }
                        break;
                    }

                case Tabs.Materials:
                    if (m_result.Data != null && m_result.VrmExtension != null)
                    {
                        m_materialEditor.OnGUI(m_importer, m_result.Data, new Vrm10TextureDescriptorGenerator(m_result.Data),
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Textures",
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Materials");

                        // render pipeline
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Experimental", MessageType.Warning);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(VrmScriptedImporter.RenderPipeline)));

                        ApplyRevertGUI();
                    }
                    break;

                case Tabs.Vrm:
                    if (m_result.Data != null && m_result.VrmExtension != null)
                    {
                        m_vrmEditor.OnGUI(m_importer, m_result.Data, m_result.VrmExtension);
                        ApplyRevertGUI();
                    }
                    break;
            }
        }
    }
}
