
using UnityEditor;
using UnityEngine;

namespace VRM
{
    [CustomEditor(typeof(VRMExportSettings))]
    public class VRMExportSettingsEditor : Editor
    {
        class CheckBoxProp
        {
            public SerializedProperty Property;
            public string Description;

            public CheckBoxProp(SerializedProperty property, string desc)
            {
                Property = property;
                Description = desc;
            }

            public void Draw()
            {
                EditorGUILayout.PropertyField(Property);
                EditorGUILayout.HelpBox(Description, MessageType.None);
                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// エクスポート時に強制的にT-Pose化する
        /// </summary>
        [Tooltip("Option")]
        public bool ForceTPose = false;

        /// <summary>
        /// エクスポート時にヒエラルキーの正規化を実施する
        /// </summary>
        [Tooltip("Require only first time")]
        public bool PoseFreeze = true;

        /// <summary>
        /// エクスポート時に新しいJsonSerializerを使う
        /// </summary>
        [Tooltip("Use new JSON serializer")]
        public bool UseExperimentalExporter = false;

        /// <summary>
        /// BlendShapeのシリアライズにSparseAccessorを使う
        /// </summary>
        [Tooltip("Use sparse accessor for blendshape. This may reduce vrm size")]
        public bool UseSparseAccessor = false;

        /// <summary>
        /// BlendShapeのPositionのみをエクスポートする
        /// </summary>
        [Tooltip("UniVRM-0.54 or later can load it. Otherwise fail to load")]
        public bool OnlyBlendshapePosition = false;

        /// <summary>
        /// エクスポート時にBlendShapeClipから参照されないBlendShapeを削除する
        /// </summary>
        [Tooltip("Remove blendshape that is not used from BlendShapeClip")]
        public bool ReduceBlendshape = false;

        /// <summary>
        /// skip if BlendShapeClip.Preset == Unknown
        /// </summary>
        [Tooltip("Remove blendShapeClip that preset is Unknown")]
        public bool ReduceBlendshapeClip = false;

        /// <summary>
        /// 頂点カラーを削除する
        /// </summary>
        [Tooltip("Remove vertex color")]
        public bool RemoveVertexColor = false;

        CheckBoxProp m_forceTPose;
        CheckBoxProp m_poseFreeze;
        CheckBoxProp m_useExcperimentalExporter;
        CheckBoxProp m_useSparseAccessor;
        CheckBoxProp m_onlyBlendShapePosition;
        CheckBoxProp m_reduceBlendShape;
        CheckBoxProp m_reduceBlendShapeClip;
        CheckBoxProp m_removeVertexColor;

        private void OnEnable()
        {
            m_forceTPose = new CheckBoxProp(serializedObject.FindProperty(nameof(ForceTPose)),
            "エクスポート時に強制的にT-Pose化する。これを使わずに手動でT-Poseを作っても問題ありません \n" +
            "Force T-Pose before export. Manually making T-Pose for model without enabling this is ok");
            m_poseFreeze = new CheckBoxProp(serializedObject.FindProperty(nameof(PoseFreeze)),
            "エクスポート時に正規化(ヒエラルキーから回転と拡大縮小を取り除くためにベイク)する \n" +
            "Model's normalization (bake to remove roation and scaling from the hierarchy)");
            m_useExcperimentalExporter = new CheckBoxProp(serializedObject.FindProperty(nameof(UseExperimentalExporter)),
            "エクスポート時に新しいJsonSerializerを使う \n" +
            "The new version of JsonSerializer for model export");
            m_useSparseAccessor = new CheckBoxProp(serializedObject.FindProperty(nameof(UseSparseAccessor)),
            "BlendShapeの容量を GLTF の Sparse Accessor 機能で削減する。修正中: UniGLTF以外でロードできません \n" +
            "BlendShape size can be reduced by using Sparse Accessor");
            m_onlyBlendShapePosition = new CheckBoxProp(serializedObject.FindProperty(nameof(OnlyBlendshapePosition)),
            "BlendShapeClipのエクスポートに法線とTangentを含めない。UniVRM-0.53 以前ではロードがエラーになるのに注意してください \n" +
            "BlendShape's Normal and Tangent will not be exported. Be aware that errors may occur during import if the model is made by UniVRM-0.53 or earlier versions");
            m_reduceBlendShape = new CheckBoxProp(serializedObject.FindProperty(nameof(ReduceBlendshape)),
            "BlendShapeClipから参照されないBlendShapeをエクスポートに含めない \n" +
            "BlendShapes that are not referenced by BlendShapeClips will not be exported");
            m_reduceBlendShapeClip = new CheckBoxProp(serializedObject.FindProperty(nameof(ReduceBlendshapeClip)),
            "BlendShapeClip.Preset == Unknown のBlendShapeClipをエクスポートに含めない \n" +
            "BlendShapeClip will not be exported if BlendShapeClip.Preset == Unknown");
            m_removeVertexColor = new CheckBoxProp(serializedObject.FindProperty(nameof(RemoveVertexColor)),
            "エクスポートに頂点カラーを含めない \n" +
            "Vertex color will not be exported");
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 160;
            serializedObject.Update();
            m_forceTPose.Draw();
            m_poseFreeze.Draw();
            m_useExcperimentalExporter.Draw();
            m_useSparseAccessor.Draw();
            m_onlyBlendShapePosition.Draw();
            m_reduceBlendShape.Draw();
            m_reduceBlendShapeClip.Draw();
            m_removeVertexColor.Draw();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
