using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniGLTF
{
    public static class BoneInfoDrawer
    {
        #region Bone
        const string BONE_GIZMO = "Assets/UniGLTF/Editor/UniGLTF/Gizmo/Models/BoneGizmo.prefab";
        static GameObject _bone;
        static GameObject BonePrefab
        {
            get
            {
                if (_bone == null)
                {
                    _bone = AssetDatabase.LoadAssetAtPath<GameObject>(BONE_GIZMO);
                }
                return _bone;
            }
        }

        private static Mesh _boneMesh;
        static Mesh BoneMesh
        {
            get
            {
                if (_boneMesh == null)
                {
                    if (BonePrefab.TryGetComponent<MeshFilter>(out var f))
                    {
                        _boneMesh = f.sharedMesh;
                    }
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
                    if (BonePrefab.TryGetComponent<MeshRenderer>(out var mr))
                    {
                        _boneMaterial = mr.sharedMaterial;
                        _boneMaterial.enableInstancing = true;
                    };
                }
                return _boneMaterial;
            }
        }
        #endregion

        #region Selected
        const string SELECTED_BONE_GIZMO = "Assets/UniGLTF/Editor/UniGLTF/Gizmo/Models/SelectedBoneGizmo.prefab";
        static GameObject _selected;
        static GameObject SelectedPrefab
        {
            get
            {
                if (_selected == null)
                {
                    _selected = AssetDatabase.LoadAssetAtPath<GameObject>(SELECTED_BONE_GIZMO);
                    if (_selected == null)
                    {
                        throw new System.NullReferenceException("SelectedPrefab");
                    }
                }
                return _selected;
            }
        }

        private static Mesh _selectedBoneMesh;
        static Mesh SelectedBoneMesh
        {
            get
            {
                if (_selectedBoneMesh == null)
                {
                    if (SelectedPrefab.TryGetComponent<MeshFilter>(out var mf))
                    {
                        _selectedBoneMesh = mf.sharedMesh;
                    };
                }
                return _selectedBoneMesh;
            }
        }

        private static Material _selectedMaterial;
        public static Material SelectedMaterial
        {
            get
            {
                if (_selectedMaterial == null)
                {
                    if (SelectedPrefab.TryGetComponent<MeshRenderer>(out var mr))
                    {
                        _selectedMaterial = mr.sharedMaterial;
                        _selectedMaterial.enableInstancing = true;
                    }
                }
                return _selectedMaterial;
            }
        }
        #endregion

        #region Hover
        const string HOVER_BONE_GIZMO = "Assets/UniGLTF/Editor/UniGLTF/Gizmo/Models/HoverBoneGizmo.prefab";
        static GameObject _hover;
        static GameObject HoverPrefab
        {
            get
            {
                if (_hover == null)
                {
                    _hover = AssetDatabase.LoadAssetAtPath<GameObject>(HOVER_BONE_GIZMO);
                    if (_hover == null)
                    {
                        throw new System.NullReferenceException("HoverPrefab");
                    }
                }
                return _hover;
            }
        }

        private static Mesh _hoverBoneMesh;
        static Mesh HoverBoneMesh
        {
            get
            {
                if (_hoverBoneMesh == null)
                {
                    if (HoverPrefab.TryGetComponent<MeshFilter>(out var mf))
                    {
                        _hoverBoneMesh = mf.sharedMesh;
                    }
                }
                return _hoverBoneMesh;
            }
        }

        private static Material _hoverMaterial;
        public static Material HoverMaterial
        {
            get
            {
                if (_hoverMaterial == null)
                {
                    if (HoverPrefab.TryGetComponent<MeshRenderer>(out var mr))
                    {
                        _hoverMaterial = mr.sharedMaterial;
                        _hoverMaterial.enableInstancing = true;
                    }
                }
                return _hoverMaterial;
            }
        }
        #endregion

        public static void DrawBone(this CommandBuffer buf, BoneInfo bone, Material material)
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

            buf.DrawMesh(SelectedBoneMesh, matrix, material);
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
