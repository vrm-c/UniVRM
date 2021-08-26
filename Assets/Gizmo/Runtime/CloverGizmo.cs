using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshUtility.Gizmo
{
    public static class CloverGizmo
    {
        private const int MaxDrawCount = 256;

        private static Mesh _boneMesh;
        private static Material _boneMaterial;

        private static Mesh _selectedBoneMesh;
        private static Material _selectedMaterial;

        private static readonly Matrix4x4[] _maxDrawAtOnceMatrices = new Matrix4x4[MaxDrawCount];

        static CloverGizmo()
        {
            var bone = Resources.Load<GameObject>("BoneGizmo");
            if (bone == null)
            {
                throw new System.Exception();
            }
            _boneMesh = bone.GetComponent<MeshFilter>().sharedMesh;
            if (_boneMesh == null)
            {
                throw new System.Exception();
            }
            _boneMaterial = bone.GetComponent<MeshRenderer>().sharedMaterial;
            _boneMaterial.enableInstancing = true;

            var selectedBone = Resources.Load<GameObject>("SelectedBoneGizmo");
            _selectedBoneMesh = selectedBone.GetComponent<MeshFilter>().sharedMesh;
            _selectedMaterial = selectedBone.GetComponent<MeshRenderer>().sharedMaterial;
            _selectedMaterial.enableInstancing = true;
        }

        public static void GetSelectedDrawBoneCommandBuffer(CommandBuffer buf, BoneInfo bone)
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

            buf.DrawMesh(_selectedBoneMesh, matrix, _selectedMaterial);
        }

        public static void GetDrawBonesCommandBuffer(CommandBuffer buf, List<BoneInfo> bones)
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

            buf.DrawMeshInstanced(_boneMesh, 0, _boneMaterial, 0, _maxDrawAtOnceMatrices);
        }
    }
}
