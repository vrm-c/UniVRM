using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.Linq;


namespace VRM
{
    [CustomEditor(typeof(VRMLookAtHead))]
    public class VRMLookAtHeadEditor : Editor
    {
        VRMLookAtHead m_target;
        PreviewRenderUtility m_previewRenderUtility;

#if UNITY_2017_1_OR_NEWER
        struct Item
        {
            public Transform Transform;
            public SkinnedMeshRenderer SkinnedMeshRenderer;
            public Mesh Mesh;
            public Material[] Materials;

            public Mesh Baked()
            {
                if (SkinnedMeshRenderer != null)
                {
                    if (Mesh == null)
                    {
                        Mesh = new Mesh();
                    }
                    SkinnedMeshRenderer.BakeMesh(Mesh);
                }
                return Mesh;
            }
        }
        Item[] m_items;

        void SetupItems()
        {
            m_items = m_target.transform.Traverse().Select(x =>
            {
                var meshFilter = x.GetComponent<MeshFilter>();
                var meshRenderer = x.GetComponent<MeshRenderer>();
                var skinnedMeshRenderer = x.GetComponent<SkinnedMeshRenderer>();
                if (meshFilter != null && meshRenderer != null)
                {
                    return new Item
                    {
                        Mesh = meshFilter.sharedMesh,
                        Transform = x.transform,
                        Materials = meshRenderer.sharedMaterials,
                    };
                }
                else if (skinnedMeshRenderer != null)
                {
                    return new Item
                    {
                        //Mesh = skinnedMeshRenderer.sharedMesh,
                        SkinnedMeshRenderer = skinnedMeshRenderer,
                        Transform = x.transform,
                        Materials = skinnedMeshRenderer.sharedMaterials
                    };
                }
                else
                {
                    return default(Item);
                }
            })
            .Where(x => x.Transform != null)
            .ToArray();
        }
#endif

        void OnEnable()
        {
            m_target = (VRMLookAtHead)target;
            m_previewRenderUtility = new PreviewRenderUtility(true);

#if UNITY_2017_1_OR_NEWER
            SetupItems();
#endif
        }

        private void OnDisable()
        {
            m_previewRenderUtility.Cleanup();
            m_previewRenderUtility = null;
        }

        static void SetPreviewCamera(Camera camera, Vector3 target, Vector3 forward)
        {
            camera.fieldOfView = 30f;
            camera.farClipPlane = 100;
            camera.nearClipPlane = 0.1f;

            camera.transform.position = target + forward * 0.8f;
            camera.transform.LookAt(target);
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            m_previewRenderUtility.BeginPreview(r, background);
            var target = m_target.Head;
            if (target != null)
            {
#if UNITY_2017_1_OR_NEWER
                SetPreviewCamera(
                    m_previewRenderUtility.camera,
                    target.position + new Vector3(0, 0.1f, 0),
                    target.forward
                    );
                for(int j=0; j<m_items.Length; ++j)
                {
                    ref var x = ref m_items[j];
                    var mesh = x.Baked();
                    for(int i=0; i<x.Materials.Length; ++i)
                    {
                        m_previewRenderUtility.DrawMesh(mesh, x.Transform.position, x.Transform.rotation,
                            x.Materials[i], i);
                    }
                }

                m_previewRenderUtility.Render();
#else
                SetPreviewCamera(
                    m_previewRenderUtility.m_Camera,
                    target.position + new Vector3(0, 0.1f, 0),
                    target.forward
                    );
                m_previewRenderUtility.m_Camera.Render();
#endif
            }
            m_previewRenderUtility.EndAndDrawPreview(r);
        }

        const float RADIUS = 0.5f;

        void OnSceneGUI()
        {
            if (m_target.Head == null) return;
            if (!m_target.DrawGizmo) return;

            if (m_target.Target != null)
            {
                {
                    EditorGUI.BeginChangeCheck();
                    var newTargetPosition = Handles.PositionHandle(m_target.Target.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(m_target.Target, "Change Look At Target Position");
                        m_target.Target.position = newTargetPosition;
                    }
                }

                Handles.color = new Color(1, 1, 1, 0.6f);
                Handles.DrawDottedLine(m_target.Head.position, m_target.Target.position, 4.0f);
            }

            Handles.matrix = m_target.Head.localToWorldMatrix;
            Handles.Label(Vector3.zero, string.Format("Yaw: {0:0.}degree\nPitch: {1:0.}degree",
                m_target.Yaw,
                m_target.Pitch));

            Handles.color = new Color(0, 1, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    Matrix4x4.identity.GetColumn(1),
                    Matrix4x4.identity.GetColumn(2),
                    m_target.Yaw,
                    RADIUS);

            Handles.matrix = m_target.Head.localToWorldMatrix * m_target.YawMatrix;
            Handles.color = new Color(1, 0, 0, 0.2f);
            Handles.DrawSolidArc(Vector3.zero,
                    Matrix4x4.identity.GetColumn(0),
                    Matrix4x4.identity.GetColumn(2),
                    -m_target.Pitch,
                    RADIUS);
        }
    }
}
