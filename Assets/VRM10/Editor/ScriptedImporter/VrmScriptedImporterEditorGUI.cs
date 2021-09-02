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

            if (expressions.Custom != null)
            {
                foreach (var kv in expressions.Custom)
                {
                    yield return ExpressionKey.CreateCustom(kv.Key).SubAssetKey;
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            var importer = target as VrmScriptedImporter;
            m_importer = importer;
            if (!Vrm10Data.TryParseOrMigrate(m_importer.assetPath, importer.MigrateToVrm1, out m_result))
            {
                // error
                return;
            }
            m_model = ModelReader.Read(m_result.Data);

            var tmp = m_importer.GetExternalObjectMap();

            var generator = new Vrm10MaterialDescriptorGenerator();
            var materialKeys = m_result.Data.GLTF.materials.Select((x, i) => generator.Get(m_result.Data, i).SubAssetKey);
            var textureKeys = new Vrm10TextureDescriptorGenerator(m_result.Data).Get().GetEnumerable().Select(x => x.SubAssetKey);
            m_materialEditor = new RemapEditorMaterial(materialKeys.Concat(textureKeys), GetEditorMap, SetEditorMap);
            m_vrmEditor = new RemapEditorVrm(new[] { VRM10Object.SubAssetKey }.Concat(EnumerateExpressinKeys(m_result.VrmExtension.Expressions)), GetEditorMap, SetEditorMap);
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
                                {
                                    serializedObject.Update();
                                    EditorGUILayout.HelpBox("Experimental", MessageType.Warning);
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(VrmScriptedImporter.RenderPipeline)));
                                    serializedObject.ApplyModifiedProperties();
                                }
                                ApplyRevertGUI();
                                break;

                            case Vrm10FileType.Vrm0:
                                EditorGUILayout.HelpBox(m_result.Message, m_model != null ? MessageType.Info : MessageType.Warning);
                                // migration check boxs
                                base.OnInspectorGUI();
                                break;

                            default:
                                ApplyRevertGUI();
                                break;
                        }
                    }
                    break;

                case Tabs.Materials:
                    if (m_result.Data != null && m_result.VrmExtension != null)
                    {
                        m_materialEditor.OnGUI(m_importer, m_result.Data, new Vrm10TextureDescriptorGenerator(m_result.Data),
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Textures",
                            assetPath => $"{Path.GetFileNameWithoutExtension(assetPath)}.vrm1.Materials");
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
