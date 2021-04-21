
using System;
using UnityEditor;
using UnityEngine;
using UniGLTF.M17N;

namespace VRM
{
    [CustomEditor(typeof(VRMExportSettings))]
    public class VRMExportSettingsEditor : Editor
    {
        class CheckBoxProp
        {
            public SerializedProperty Property;
            public Func<string> Description;

            public CheckBoxProp(SerializedProperty property, Func<string> desc)
            {
                Property = property;
                Description = desc;
            }

            public CheckBoxProp(SerializedProperty property, Options desc) : this(property, () => Msg(desc))
            {
            }

            public CheckBoxProp(SerializedProperty property, string desc) : this(property, () => desc)
            {
            }

            public void Draw()
            {
                EditorGUILayout.PropertyField(Property);
                EditorGUILayout.HelpBox(Description(), MessageType.None);
                EditorGUILayout.Space();
            }
        }

        CheckBoxProp m_poseFreeze;
        CheckBoxProp m_useSparseAccessor;
        CheckBoxProp m_onlyBlendShapePosition;
        CheckBoxProp m_reduceBlendShape;
        CheckBoxProp m_reduceBlendShapeClip;
        CheckBoxProp m_divideVertexBuffer;

        static string Msg(Options key)
        {
            return LanguageGetter.Msg(key);
        }

        public enum Options
        {
            [LangMsg(Languages.ja, "エクスポート時に強制的にT-Pose化する。これを使わずに手動でT-Poseを作っても問題ありません")]
            [LangMsg(Languages.en, "Force T-Pose before export. Manually making T-Pose for model without enabling this is ok")]
            FORCE_T_POSE,

            [LangMsg(Languages.ja, "エクスポート時に正規化(ヒエラルキーから回転と拡大縮小を取り除くためにベイク)する")]
            [LangMsg(Languages.en, "Model's normalization (bake to remove roation and scaling from the hierarchy)")]
            NORMALIZE,

            [LangMsg(Languages.ja, "エクスポート時に新しいJsonSerializerを使う")]
            [LangMsg(Languages.en, "The new version of JsonSerializer for model export")]
            USE_GENERATED_SERIALIZER,

            [LangMsg(Languages.ja, "BlendShapeの容量を GLTF の Sparse Accessor 機能で削減する。修正中: UniGLTF以外でロードできません")]
            [LangMsg(Languages.en, "BlendShape size can be reduced by using Sparse Accessor")]
            BLENDSHAPE_USE_SPARSE,

            [LangMsg(Languages.ja, "BlendShapeClipのエクスポートに法線とTangentを含めない。UniVRM-0.53 以前ではロードがエラーになるのに注意してください")]
            [LangMsg(Languages.en, "BlendShape's Normal and Tangent will not be exported. Be aware that errors may occur during import if the model is made by UniVRM-0.53 or earlier versions")]
            BLENDSHAPE_EXCLUDE_NORMAL_AND_TANGENT,

            [LangMsg(Languages.ja, "BlendShapeClipから参照されないBlendShapeをエクスポートに含めない")]
            [LangMsg(Languages.en, "BlendShapes that are not referenced by BlendShapeClips will not be exported")]
            BLENDSHAPE_ONLY_CLIP_USE,

            [LangMsg(Languages.ja, "BlendShapeClip.Preset == Unknown のBlendShapeClipをエクスポートに含めない")]
            [LangMsg(Languages.en, "BlendShapeClip will not be exported if BlendShapeClip.Preset == Unknown")]
            BLENDSHAPE_EXCLUDE_UNKNOWN,

            [LangMsg(Languages.ja, "エクスポートに頂点カラーを含めない")]
            [LangMsg(Languages.en, "Vertex color will not be exported")]
            REMOVE_VERTEX_COLOR,

            [LangMsg(Languages.ja, "T-Pose にする")]
            [LangMsg(Languages.en, "Make T-Pose")]
            DO_TPOSE,

            [LangMsg(Languages.ja, "頂点バッファをsubmeshで分割する。GLTF互換性のため。UniVRM-0.72 からロードできる。")]
            [LangMsg(Languages.en, "Divide vertex buffer by submesh。For more gltf compatibility。UniVRM-0.72 or later can load.")]
            DIVIDE_VERTEX_BUFFER,
        }

        private void OnEnable()
        {
            m_poseFreeze = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.PoseFreeze)), Options.NORMALIZE);
            m_useSparseAccessor = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.UseSparseAccessor)), Options.BLENDSHAPE_USE_SPARSE);
            m_onlyBlendShapePosition = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.OnlyBlendshapePosition)), Options.BLENDSHAPE_EXCLUDE_NORMAL_AND_TANGENT);
            m_reduceBlendShape = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.ReduceBlendshape)), Options.BLENDSHAPE_ONLY_CLIP_USE);
            m_reduceBlendShapeClip = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.ReduceBlendshapeClip)), Options.BLENDSHAPE_EXCLUDE_UNKNOWN);
            m_divideVertexBuffer = new CheckBoxProp(serializedObject.FindProperty(nameof(VRMExportSettings.DivideVertexBuffer)), Options.DIVIDE_VERTEX_BUFFER);
        }


        public override void OnInspectorGUI()
        {
            GUILayout.Space(20);
            var settings = (VRMExportSettings)target;
            var root = settings.Root;

            // ToDo: 任意の BlendShapeClip を適用する

            EditorGUIUtility.labelWidth = 160;
            serializedObject.Update();
            m_poseFreeze.Draw();
            m_useSparseAccessor.Draw();
            m_onlyBlendShapePosition.Draw();
            m_reduceBlendShape.Draw();
            m_reduceBlendShapeClip.Draw();
            m_divideVertexBuffer.Draw();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
