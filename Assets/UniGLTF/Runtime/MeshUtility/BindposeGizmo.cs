using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// using UniGLTF;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF.MeshUtility
{
    [DisallowMultipleComponent]
    public class BindposeGizmo : MonoBehaviour
    {
        [SerializeField]
        Mesh m_target;

        [SerializeField]
        float[] m_boneWeights;

        [SerializeField, Range(0.1f, 1.0f)]
        float m_gizmoSize = 0.02f;

        [SerializeField]
        Color m_meshGizmoColor = Color.yellow;

        [SerializeField]
        Color m_bindGizmoColor = Color.red;

        private void Reset()
        {
            var renderer = GetComponent<SkinnedMeshRenderer>();
            if (renderer == null) return;
            m_target = renderer.sharedMesh;
        }

#if UNITY_EDITOR
        #region ToBindpose
        [ContextMenu("ToBindpose")]
        void ToBindpose()
        {
            var renderer = GetComponent<SkinnedMeshRenderer>();

            var root =
            renderer.bones
                .Select(x => Ancestors(x).Reverse().ToArray())
                .Aggregate((a, b) =>
                {
                    int i = 0;
                    for (; i < a.Length && i < b.Length; ++i)
                    {
                        if (a[i] != b[i])
                        {
                            break;
                        }
                    }
                    return a.Take(i).ToArray();
                })
                .Last()
                ;

            var map = new Dictionary<Transform, Matrix4x4>();
            for (int i = 0; i < renderer.bones.Length; ++i)
            {
                map[renderer.bones[i]] = m_target.bindposes[i];
            }

            {
                var bones = Traverse(root);
                Undo.RecordObjects(bones.ToArray(), "toBindpose");

                foreach (var x in bones)
                {
                    var bind = default(Matrix4x4);
                    if (map.TryGetValue(x, out bind))
                    {
                        var toWorld = renderer.transform.localToWorldMatrix * bind.inverse;
                        x.position = toWorld.GetColumn(3);
                        x.rotation = toWorld.ExtractRotation();
                    }
                }

                //EditorUtility.SetDirty(transform);
            }
        }

        IEnumerable<Transform> Traverse(Transform self)
        {
            yield return self;

            foreach (Transform child in self)
            {
                foreach (var x in Traverse(child))
                {
                    yield return x;
                }
            }
        }

        IEnumerable<Transform> Ancestors(Transform self)
        {
            yield return self;

            if (self.parent != null)
            {
                foreach (var x in Ancestors(self.parent))
                {
                    yield return x;
                }
            }
        }
        #endregion
#endif

        private void OnDrawGizmos()
        {
            if (m_target == null)
            {
                return;
            }

            Gizmos.matrix = transform.localToWorldMatrix;

            if (m_target.bindposes != null && m_target.bindposes.Length > 0)
            {
                if (m_boneWeights == null || m_boneWeights.Length != m_target.bindposes.Length)
                {
                    m_boneWeights = new float[m_target.bindposes.Length];
                    foreach (var bw in m_target.boneWeights)
                    {
                        if (bw.weight0 > 0) m_boneWeights[bw.boneIndex0] += bw.weight0;
                        if (bw.weight1 > 0) m_boneWeights[bw.boneIndex1] += bw.weight1;
                        if (bw.weight2 > 0) m_boneWeights[bw.boneIndex2] += bw.weight2;
                        if (bw.weight3 > 0) m_boneWeights[bw.boneIndex3] += bw.weight3;
                    }
                }

                Gizmos.color = m_meshGizmoColor;
                Gizmos.DrawWireMesh(m_target);

                for (var i = 0; i < m_target.bindposes.Length; ++i)
                {
                    var color = m_bindGizmoColor * m_boneWeights[i];
                    color.a = 1.0f;
                    Gizmos.color = color;

                    Gizmos.matrix = transform.localToWorldMatrix * m_target.bindposes[i].inverse;
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one * m_gizmoSize);
                }
            }
            else
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireMesh(m_target);
            }
        }
    }
}
