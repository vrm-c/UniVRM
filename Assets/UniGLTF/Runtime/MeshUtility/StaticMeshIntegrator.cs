using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UniGLTF.MeshUtility
{
    public static class StaticMeshIntegrator
    {
        const string ASSET_SUFFIX = ".mesh.asset";

        class Integrator
        {
            List<Vector3> m_positions = new List<Vector3>();
            List<Vector3> m_normals = new List<Vector3>();
            List<Vector2> m_uv = new List<Vector2>();
            /*
            List<Vector2> m_uv2 = new List<Vector2>(); // ToDo
            List<Vector2> m_uv3 = new List<Vector2>(); // ToDo
            List<Vector2> m_uv4 = new List<Vector2>(); // ToDo
            List<Color> m_colors = new List<Color>(); // ToDo
            */

            List<int[]> m_subMeshes = new List<int[]>();

            List<Material> m_materials = new List<Material>();
            public List<Material> Materials
            {
                get { return m_materials; }
            }

            public void Push(Matrix4x4 localToRoot, Mesh mesh, Material[] materials)
            {
                var offset = m_positions.Count;

                var hasNormal = m_normals.Count == m_positions.Count;
                var hasUv = m_uv.Count == m_positions.Count;

                // attributes
                m_positions.AddRange(mesh.vertices.Select(x => localToRoot.MultiplyPoint(x)));
                if(mesh.normals!=null && mesh.normals.Length == mesh.vertexCount)
                {
                    if (!hasNormal) for (int i = m_normals.Count; i < m_positions.Count; ++i) m_normals.Add(Vector3.zero);
                    m_normals.AddRange(mesh.normals.Select(x => localToRoot.MultiplyVector(x)));
                }
                if (mesh.uv != null && mesh.uv.Length == mesh.vertexCount)
                {
                    if (!hasUv) for (int i = m_uv.Count; i < m_positions.Count; ++i) m_uv.Add(Vector2.zero);
                    m_uv.AddRange(mesh.uv);
                }

                // indices
                for (int i = 0; i < mesh.subMeshCount; ++i)
                {
                    m_subMeshes.Add(mesh.GetIndices(i).Select(x => offset + x).ToArray());
                }

                // materials
                m_materials.AddRange(materials);
            }

            public Mesh ToMesh()
            {
                var mesh = new Mesh();
                mesh.name = MeshIntegratorUtility.INTEGRATED_MESH_NAME;

                mesh.vertices = m_positions.ToArray();
                if (m_normals.Count > 0)
                {
                    if (m_normals.Count < m_positions.Count) for (int i = m_normals.Count; i < m_positions.Count; ++i) m_normals.Add(Vector3.zero);
                    mesh.normals = m_normals.ToArray();
                }
                if (m_uv.Count > 0)
                {
                    if (m_uv.Count < m_positions.Count) for (int i = m_uv.Count; i < m_positions.Count; ++i) m_uv.Add(Vector2.zero);
                    mesh.uv = m_uv.ToArray();
                }

                mesh.subMeshCount = m_subMeshes.Count;
                for(int i=0; i<m_subMeshes.Count; ++i)
                {
                    mesh.SetIndices(m_subMeshes[i], MeshTopology.Triangles, i);
                }

                return mesh;
            }
        }

        public struct MeshWithMaterials
        {
            public Mesh Mesh;
            public Material[] Materials;
        }

        public static MeshWithMaterials Integrate(Transform root)
        {
            var integrator = new Integrator();

            foreach (var t in root.Traverse())
            {
                var renderer = t.GetComponent<MeshRenderer>();
                var filter = t.GetComponent<MeshFilter>();
                if (renderer != null &&  filter != null && filter.sharedMesh != null 
                    && renderer.sharedMaterials!=null && renderer.sharedMaterials.Length == filter.sharedMesh.subMeshCount)
                {
                    integrator.Push(root.worldToLocalMatrix * t.localToWorldMatrix, filter.sharedMesh, renderer.sharedMaterials);
                }
            }

            return new MeshWithMaterials
            {
                Mesh = integrator.ToMesh(),
                Materials = integrator.Materials.ToArray(),
            };
        }

    }
}
