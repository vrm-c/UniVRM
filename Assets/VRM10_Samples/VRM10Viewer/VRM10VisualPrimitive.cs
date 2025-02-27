using UnityEngine;
using UnityEngine.Rendering;

namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// Built-in RP と URP の差異を楽に吸収してプリミティブを表示するためのクラス
    /// </summary>
    public class VRM10VisualPrimitive : MonoBehaviour
    {
        [SerializeField] private PrimitiveType _primitiveType;

        /// 'Always Inlucded Shaders` に `Universal Render Pipeline/Lit` を指定することが現実的でないため指定する。
        /// 簡易なシェーダーで十分です。
        /// ビルドするときに必要です。Editorでは無くても表示できるかもしれません。
        [SerializeField] private Material _urpMaterialForGrayscale;

        public PrimitiveType PrimitiveType
        {
            get => _primitiveType;
            set => _primitiveType = value;
        }

        private void Start()
        {
            var visual = GameObject.CreatePrimitive(_primitiveType);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            // URP 判定
            if (GraphicsSettings.renderPipelineAsset != null
                // WebGL ビルドでは GraphicsSettings.renderPipelineAsset が常に null ?
                || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                if (_urpMaterialForGrayscale != null)
                {
                    var m = Instantiate(_urpMaterialForGrayscale);
                    m.SetFloat("_Metallic", 0);
                    m.SetFloat("_Roughness", 1);
                    visual.GetComponent<Renderer>().material = m;
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                switch (_primitiveType)
                {
                    case PrimitiveType.Sphere:
                        Gizmos.DrawSphere(Vector3.zero, 1.0f);
                        break;

                    case PrimitiveType.Plane:
                        Gizmos.DrawWireCube(Vector3.zero, new Vector3(10, 0.001f, 10));
                        break;

                    default:
                        // TODO
                        break;
                }
            }
        }
    }
}