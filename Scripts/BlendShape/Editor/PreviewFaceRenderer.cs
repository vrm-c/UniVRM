using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniGLTF;
using System.Reflection;

namespace VRM
{
    public class PreviewFaceRenderer : IDisposable
    {
        const string PreviewInstanceName = "FaceMorphemePreviewInstance";

        PreviewRenderUtility m_previewUtility;
        GameObject m_prefab;
        GameObject m_previewInstance;
        SkinnedMeshRenderer[] m_Renderers;
        Mesh[] m_baked;
        Bounds m_bounds;

        void SetupCamera(Camera camera)
        {
            float magnitude = m_bounds.extents.magnitude;
            float distance = 4f * magnitude;
            camera.fieldOfView = 27f;
            camera.backgroundColor = Color.gray;
            camera.clearFlags = CameraClearFlags.Color;
            // this used to be "-Vector3.forward * num" but I hardcoded my camera position instead
            camera.transform.position = new Vector3(0f, 1.4f, 1.5f); 
            camera.transform.rotation = Quaternion.Euler(0, 180f, 0);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = distance + magnitude * 1.1f;
        }

        public PreviewFaceRenderer(GameObject prefab)
        {
            m_prefab = prefab;
            m_previewUtility = new PreviewRenderUtility();

            // if we already instantiated a PreviewInstance previously but just lost the reference, then use that same instance instead of making a new one
            var oldInstance = GameObject.Find(PreviewInstanceName);
            if (oldInstance != null)
            {
                m_previewInstance = oldInstance;
            }
            else
            { // no previous instance detected, so now let's make a fresh one
              // very important: this loads the PreviewInstance prefab and temporarily instantiates it into PreviewInstance
                m_previewInstance = (GameObject)GameObject.Instantiate(m_prefab, m_prefab.transform.position, m_prefab.transform.rotation);
                m_previewInstance.name = PreviewInstanceName;
                // HideFlags are special editor-only settings that let you have *secret* GameObjects in a scene, or to tell Unity not to save that temporary GameObject as part of the scene
                m_previewInstance.hideFlags =
                     HideFlags.HideAndDontSave
                //HideFlags.DontSaveInEditor 
                //| HideFlags.DontSaveInBuild
                ; // you could also hide it from the hierarchy or inspector, but personally I like knowing everything that's there

                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                var propInfo = typeof(Camera).GetProperty("PreviewCullingLayer", flags);
                int previewLayer = (int)propInfo.GetValue(null, new object[0]);
                //previewLayer のみ表示する
                m_previewUtility.m_Camera.cullingMask = 1 << previewLayer;

                foreach (var x in m_previewInstance.transform.Traverse())
                {
                    x.gameObject.layer = previewLayer;
                }
            }

            m_Renderers = m_previewInstance.transform.Traverse()
                .Select(x => x.GetComponent<SkinnedMeshRenderer>())
                .Where(x => x != null && x.sharedMesh != null)
                .ToArray();
            m_baked = m_Renderers.Select(x => x.sharedMesh)
                .ToArray();
            for (int i = 0; i < m_Renderers.Length; ++i)
            {
                if (m_baked[i] == null)
                {
                    m_baked[i] = new Mesh();
                }
                m_Renderers[i].BakeMesh(m_baked[i]);

                m_bounds.Expand(m_baked[i].bounds.min);
                m_bounds.Expand(m_baked[i].bounds.max);
            }
        }

        public Texture Render(Rect r, GUIStyle background)
        {
            // we are technically rendering everything in the scene, so scene fog might affect it...
            bool fog = RenderSettings.fog; // ... let's remember the current fog setting...
            Unsupported.SetRenderSettingsUseFogNoDirty(false); // ... and then temporarily turn it off
            try
            {
                m_previewUtility.BeginPreview(r, background); // set up the PreviewRenderUtility's mini internal scene

                _Render();

                // VERY IMPORTANT: this manually tells the camera to render and produce the render texture
                m_previewUtility.m_Camera.Render();

                // reset the scene's fog from before
                return m_previewUtility.EndPreview(); // grab the RenderTexture resulting from DoRenderPreview() > RenderMeshPreview() > PreviewRenderUtility.m_Camera.Render()
            }
            finally
            {
                Unsupported.SetRenderSettingsUseFogNoDirty(fog);
            }
        }

        private void _Render()
        {
            // setup the ObjectPreview's camera
            SetupCamera(m_previewUtility.m_Camera);

            // very important: we have to call BakeMesh, to bake that animated SkinnedMesh into a plain static Mesh suitable for Graphics.DrawMesh()
            for(int j=0; j<m_Renderers.Length; ++j)
            {
                var renderer = m_Renderers[j];
                var mesh = m_baked[j];

                // now, actually render out the RenderTexture
                //RenderMeshPreview(previewMesh, skinMeshRender.sharedMaterials);
                // submesh support, in case the mesh is made of multiple parts
                int subMeshCount = mesh.subMeshCount;
                for (int i = 0; i < subMeshCount; i++)
                {
                    // PreviewRenderUtility.DrawMesh() actually draws the mesh
                    m_previewUtility.DrawMesh(mesh, 
                        renderer.transform.position, renderer.transform.rotation, 
                        renderer.sharedMaterials[i], i);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                    if (this.m_previewUtility != null)
                    {
                        this.m_previewUtility.Cleanup();
                        this.m_previewUtility = null;
                    }
                    if (m_previewInstance != null)
                    {
                        GameObject.DestroyImmediate(m_previewInstance);
                    }
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~PreviewFaceRenderer() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
