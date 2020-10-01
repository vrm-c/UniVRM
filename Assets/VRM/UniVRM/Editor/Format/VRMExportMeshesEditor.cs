
using System;
using UnityEditor;
using UnityEngine;
using VRM.M17N;

namespace VRM
{
    [CustomEditor(typeof(VRMExportMeshes))]
    public class VRMExportMeshesEditor : Editor
    {
        VRMExportMeshes m_meshes;
        private void OnEnable()
        {
            m_meshes = target as VRMExportMeshes;
            // m_forceTPose = new CheckBoxProp(serializedObject.FindProperty(nameof(ForceTPose)), Options.FORCE_T_POSE);
            // m_poseFreeze = new CheckBoxProp(serializedObject.FindProperty(nameof(PoseFreeze)), Options.NORMALIZE);
            // m_useSparseAccessor = new CheckBoxProp(serializedObject.FindProperty(nameof(UseSparseAccessor)), Options.BLENDSHAPE_USE_SPARSE);
            // m_onlyBlendShapePosition = new CheckBoxProp(serializedObject.FindProperty(nameof(OnlyBlendshapePosition)), Options.BLENDSHAPE_EXCLUDE_NORMAL_AND_TANGENT);
            // m_reduceBlendShape = new CheckBoxProp(serializedObject.FindProperty(nameof(ReduceBlendshape)), Options.BLENDSHAPE_ONLY_CLIP_USE);
            // m_reduceBlendShapeClip = new CheckBoxProp(serializedObject.FindProperty(nameof(ReduceBlendshapeClip)), Options.BLENDSHAPE_EXCLUDE_UNKNOWN);
            // m_removeVertexColor = new CheckBoxProp(serializedObject.FindProperty(nameof(RemoveVertexColor)), Options.REMOVE_VERTEX_COLOR);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox($"Mesh size: {m_meshes.ExpectedExportByteSize / 1000000.0f:0.0} MByte", MessageType.Info);
        }
    }
}
