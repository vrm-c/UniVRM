using System;
using UnityEngine;

namespace VRM10.Samples.MToon10Showcase
{
    public sealed class TopDownCapture : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        [SerializeField] private RenderTexture _renderTarget;
        
        public RenderTexture RenderTarget => _renderTarget;

        public void Capture(Vector3 topLeft, Vector3 bottomRight)
        {
            if (topLeft.x > bottomRight.x) throw new ArgumentException("TopLeft.x must be less than BottomRight.x");
            if (topLeft.z < bottomRight.z) throw new ArgumentException("TopLeft.z must be greater than BottomRight.z");

            var width = bottomRight.x - topLeft.x;
            var height = topLeft.z - bottomRight.z;
            var aspect = width / height;
            _camera.transform.position = new Vector3(topLeft.x + width / 2f, topLeft.y, topLeft.z - height / 2f);
            _camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _camera.orthographic = true;
            _camera.orthographicSize = height / 2f;
            var texWidth = 480;
            var texHeight = (int)(texWidth / aspect);

            if (texWidth * texHeight > 4096 * 4096) throw new Exception("Too many pixels to capture.");

            _renderTarget = new RenderTexture(texWidth, texHeight, 16, RenderTextureFormat.ARGB32);
            _camera.targetTexture = _renderTarget;
            _camera.enabled = true;
            _camera.Render();

            _camera.enabled = false;
            _camera.targetTexture = null;
        }
    }
}