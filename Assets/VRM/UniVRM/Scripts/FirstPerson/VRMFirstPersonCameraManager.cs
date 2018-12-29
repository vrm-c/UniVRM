#pragma warning disable 0414, 0649
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

namespace VRM
{
    /// <summary>
    /// ファーストパーソン向けLayer検討
    /// 
    /// * Deault LayerをFirstPersonレイヤーとして使う
    /// * 9番にThirdPerson Layerを追加する
    /// 
    /// * FirstPersonCameraはCullingMaskでThirdPerson Layerを除外
    /// * ThirdPersonCameraはCullingMaskでDefault Layerを除外
    /// 
    /// * それ以外のシーンオブジェクトはDefaultLayerとThirdPersonレイヤーの両方に所属するべし
    /// * 首無しモデルはDefault Layerのみに所属するべし
    /// * 首有りモデルはThirdPerson Layerのみに所属するべし
    /// * コントローラーはDefault Layerがいいかも
    /// * 鏡もDefault Layerがいいかも(カメラごとにRenderTargetを用意するのは煩雑)
    /// </summary>
    public class VRMFirstPersonCameraManager : MonoBehaviour
    {
        [Serializable]
        class CameraWithRawImage
        {
            public Camera Camera;
            public RenderTexture Texture;
            public RawImage Image;
        }

        /// <summary>
        /// FirstPerson
        /// </summary>
        [SerializeField]
        CameraWithRawImage m_topLeft;

        /// <summary>
        /// ThirdPerson body
        /// </summary>
        [SerializeField]
        CameraWithRawImage m_topRight;

        /// <summary>
        /// ThirdPerson head
        /// </summary>
        [SerializeField]
        CameraWithRawImage m_bottomRight;

        [SerializeField, Header("Cameras")]
        Camera m_firstPersonCamera;

        [SerializeField]
        Camera[] m_thirdPersonCameras;

        void Reset()
        {
            var cameras = GameObject.FindObjectsOfType<Camera>();
            m_firstPersonCamera = Camera.main;
            m_thirdPersonCameras = cameras.Where(x => x != m_firstPersonCamera).ToArray();
        }

        private void Update()
        {
            var halfWidth = Screen.width / 2;
            var halfHeight = Screen.height / 2;
            SetupRenderTarget(m_topLeft, halfWidth, halfHeight);
            SetupRenderTarget(m_topRight, halfWidth, halfHeight);
            SetupRenderTarget(m_bottomRight, halfWidth, halfHeight);
        }

        void SetupRenderTarget(CameraWithRawImage cameraWithImage, int w, int h)
        {
            if (cameraWithImage.Camera == null) return;
            if (cameraWithImage.Image == null) return;

            if (cameraWithImage.Texture == null 
                || cameraWithImage.Texture.width != w 
                || cameraWithImage.Texture.height != h
                )
            {
                var texture = new RenderTexture(w, h, 16);
                cameraWithImage.Texture = texture;
                cameraWithImage.Camera.targetTexture = texture;
                cameraWithImage.Image.texture = texture;
            }
        }
    }
}
