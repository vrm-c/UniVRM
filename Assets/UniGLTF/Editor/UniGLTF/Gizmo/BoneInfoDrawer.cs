using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public static class BoneInfoDrawer
    {
        const string BONE_GIZMO = "Assets/UniGLTF/Editor/UniGLTF/Gizmo/Models/BoneGizmo.prefab";
        const string SELECTED_BONE_GIZMO = "Assets/UniGLTF/Editor/UniGLTF/Gizmo/Models/SelectedBoneGizmo.prefab";

        private static Mesh _boneMesh;
        static Mesh BoneMesh
        {
            get
            {
                if (_boneMesh == null)
                {
                    var bone = AssetDatabase.LoadAssetAtPath<GameObject>(BONE_GIZMO);
                    _boneMesh = bone.GetComponent<MeshFilter>().sharedMesh;
                }
                return _boneMesh;
            }
        }

        private static Material _boneMaterial;
        static Material BoneMaterial
        {
            get
            {
                if (_boneMaterial == null)
                {
                    var bone = AssetDatabase.LoadAssetAtPath<GameObject>(BONE_GIZMO);
                    _boneMaterial = bone.GetComponent<MeshRenderer>().sharedMaterial;
                    _boneMaterial.enableInstancing = true;
                }
                return _boneMaterial;
            }
        }

        private static Mesh _selectedBoneMesh;
        static Mesh SelectedBoneMesh
        {
            get
            {
                if (_selectedBoneMesh)
                {
                    var selectedBone = AssetDatabase.LoadAssetAtPath<GameObject>(SELECTED_BONE_GIZMO);
                    _selectedBoneMesh = selectedBone.GetComponent<MeshFilter>().sharedMesh;
                }
                return _selectedBoneMesh;
            }
        }

        private static Material _selectedMaterial;
        static Material SelectedMaterial
        {
            get
            {
                if (_selectedMaterial)
                {
                    var selectedBone = AssetDatabase.LoadAssetAtPath<GameObject>(SELECTED_BONE_GIZMO);
                    _selectedMaterial = selectedBone.GetComponent<MeshRenderer>().sharedMaterial;
                    _selectedMaterial.enableInstancing = true;
                }
                return _selectedMaterial;
            }
        }

        public static void DrawBone(this CommandBuffer buf, BoneInfo bone)
        {
            var head = bone.GetHeadPosition();
            var tail = bone.GetTailPosition();

            var headToTail = tail - head;
            var distance = headToTail.magnitude;

            var matrix = Matrix4x4.TRS(
                head,
                Quaternion.LookRotation(headToTail, bone.GetUpVector()),
                new Vector3(distance, distance, distance)
            );

            buf.DrawMesh(SelectedBoneMesh, matrix, SelectedMaterial);
        }

        private const int MaxDrawCount = 256;
        private static readonly Matrix4x4[] _maxDrawAtOnceMatrices = new Matrix4x4[MaxDrawCount];
        public static void DrawBones(this CommandBuffer buf, List<BoneInfo> bones)
        {
            var idx = 0;
            foreach (var bone in bones)
            {
                if (idx >= MaxDrawCount) break;

                var head = bone.GetHeadPosition();
                var tail = bone.GetTailPosition();

                var headToTail = tail - head;
                var distance = headToTail.magnitude;

                _maxDrawAtOnceMatrices[idx++] = Matrix4x4.TRS(
                    head,
                    Quaternion.LookRotation(headToTail, bone.GetUpVector()),
                    new Vector3(distance, distance, distance)
                );
            }
            buf.DrawMeshInstanced(BoneMesh, 0, BoneMaterial, 0, _maxDrawAtOnceMatrices);
        }
    }
}
