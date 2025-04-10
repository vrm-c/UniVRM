using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace VRM10.Samples.MToon10Showcase
{
    public sealed class TopDownCapture : MonoBehaviour
    {
        private static readonly float CameraHeight = 5f;
        private static readonly int MaxPixels = 4096 * 4096;
        private static readonly int TextureWidth = 480;

        [SerializeField] private Camera _camera;

        private RenderTexture _renderTarget;
        private GameObject _quad;
        private Vector3 _topLeftOfCaptureArea;
        private Vector3 _bottomRightOfCaptureArea;

        public void Capture(Vector3 topLeft, Vector3 bottomRight)
        {
            if (topLeft.x > bottomRight.x) throw new ArgumentException("TopLeft.x must be less than BottomRight.x");
            if (topLeft.z < bottomRight.z) throw new ArgumentException("TopLeft.z must be greater than BottomRight.z");

            // Set up render texture
            var width = bottomRight.x - topLeft.x;
            var height = topLeft.z - bottomRight.z;
            var aspect = width / height;
            var texHeight = (int)(TextureWidth / aspect);
            if (TextureWidth * texHeight > MaxPixels) throw new Exception("Too many pixels to capture.");
            if (_renderTarget != null) _renderTarget.Release();
            _renderTarget = new RenderTexture(TextureWidth, texHeight, 16, RenderTextureFormat.ARGB32);

            // Set up camera
            _camera.transform.position = new Vector3(topLeft.x + width / 2f, CameraHeight, topLeft.z - height / 2f);
            _camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _camera.orthographic = true;
            _camera.orthographicSize = height / 2f;
            _camera.targetTexture = _renderTarget;
            
            // Capture
            _camera.Render();

            // Save capture area for next step
            _topLeftOfCaptureArea = topLeft;
            _bottomRightOfCaptureArea = bottomRight;
        }

        public void ShowCapturedImageNextToCaptureTarget(float xSpacing)
        {
            if (_renderTarget == null)
            {
                Debug.LogWarning("Call Capture() before ShowCaptureOnMesh()");
                return;
            }

            if (_quad != null)
            {
                return;
            }

            var offset = new Vector3(_bottomRightOfCaptureArea.x + xSpacing, 0, 0);
            var topLeft = _topLeftOfCaptureArea + offset;
            var bottomRight = _bottomRightOfCaptureArea + offset;
            var width = bottomRight.x - topLeft.x;
            var height = topLeft.z - bottomRight.z;
            _quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _quad.gameObject.name = "Capture";
            _quad.transform.position = new Vector3(topLeft.x + width / 2f, topLeft.y, topLeft.z - height / 2f);
            _quad.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _quad.transform.localScale = new Vector3(width, height, 1f);
            var material = new Material(Shader.Find("Unlit/Texture"));
            material.mainTexture = _renderTarget;
            _quad.GetComponent<Renderer>().material = material;
        }

        public void ExportCapturedImage(string path)
        {
            if (_renderTarget == null)
            {
                Debug.LogWarning("Call Capture() before ExportCapturedImage()");
                return;
            }

            AsyncGPUReadback.Request(_renderTarget, 0, request =>
            {
                if (request.hasError)
                {
                    Debug.LogError("AsyncGPUReadback error");
                    return;
                }

                var texture = new Texture2D(_renderTarget.width, _renderTarget.height, TextureFormat.RGBA32, false);
                var data = request.GetData<Color32>();
                texture.LoadRawTextureData(data);
                texture.Apply();
                var encodedData = texture.EncodeToPNG();
                File.WriteAllBytes(path, encodedData);
                DestroyImmediate(texture);
            });
        }
    }
}