using System;
using UnityEditor;
using UnityEngine;


namespace VRM
{
    /// <summary>
    /// based
    /// 
    /// * https://gist.github.com/radiatoryang/a2282d44ba71848e498bb2e03da98991
    /// </summary>

    /// <summary>
    /// PreviewRenderUtilityを管理する
    /// PreviewSceneをレンダリングする
    /// </summary>
    public class PreviewFaceRenderer : IDisposable
    {
        PreviewRenderUtility m_previewUtility;
        public Camera PreviewCamera
        {
            get
            {
#if UNITY_2017_1_OR_NEWER
                return m_previewUtility.camera;
#else
                return m_previewUtility.m_Camera;
#endif
            }
        }

        public Light[] PreviewLights
        {
            get
            {
#if UNITY_2017_1_OR_NEWER
                return m_previewUtility.lights;
#else
                return m_previewUtility.m_Light;
#endif
            }
        }

        public void SetAmbientColor(Color color)
        {
#if UNITY_2017_1_OR_NEWER
            m_previewUtility.ambientColor = color;
#else
            // ?
#endif
        }

        public PreviewFaceRenderer()
        {
            m_previewUtility = new PreviewRenderUtility();

            foreach (var light in PreviewLights)
            {
                if (light == null) continue;
                light.intensity = 0f;
            }

            if (PreviewLights.Length > 0 && PreviewLights[0] != null)
            {
                PreviewLights[0].intensity = 1f;
                PreviewLights[0].transform.rotation = Quaternion.Euler(20f, 200f, 0);
                PreviewLights[0].color = new Color(1f, 1f, 1f, 1f);
            }

            SetAmbientColor(new Color(0.1f, 0.1f, 0.1f, 1f));
        }

        class FogScope : IDisposable
        {
            bool fog;

            public FogScope()
            {
                fog = RenderSettings.fog; // ... let's remember the current fog setting...
                // we are technically rendering everything in the scene, so scene fog might affect it...
                Unsupported.SetRenderSettingsUseFogNoDirty(false); // ... and then temporarily turn it off
            }

            public void Dispose()
            {
                Unsupported.SetRenderSettingsUseFogNoDirty(fog);
            }
        }

        //const float FACTOR = 0.1f;

        public Texture Render(Rect r, GUIStyle background, PreviewSceneManager scene,
        float yaw, float pitch, Vector3 position)
        {
            if (scene == null) return null;

            using (var fog = new FogScope())
            {
                m_previewUtility.BeginPreview(r, background); // set up the PreviewRenderUtility's mini internal scene

                // setup the ObjectPreview's camera
                scene.SetupCamera(PreviewCamera, scene.TargetPosition, yaw, pitch, position);

                foreach (var item in scene.EnumRenderItems)
                {
                    // now, actually render out the RenderTexture
                    //RenderMeshPreview(previewMesh, skinMeshRender.sharedMaterials);
                    // submesh support, in case the mesh is made of multiple parts
                    int subMeshCount = item.Mesh.subMeshCount;
                    for (int i = 0; i < subMeshCount; i++)
                    {
                        m_previewUtility.DrawMesh(item.Mesh,
                            item.Position, item.Rotation,
                            item.Materials[i], i);
                    }
                }

                // VERY IMPORTANT: this manually tells the camera to render and produce the render texture
                PreviewCamera.Render();
                //m_previewUtility.Render(false, false);

                // reset the scene's fog from before
                return m_previewUtility.EndPreview();
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
