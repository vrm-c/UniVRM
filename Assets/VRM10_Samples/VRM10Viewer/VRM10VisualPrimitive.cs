using UnityEngine;

namespace UniVRM10.VRM10Viewer
{
    /// <summary>
    /// Built-in RP と URP の差異を楽に吸収してプリミティブを表示するためのクラス
    /// </summary>
    public class VRM10VisualPrimitive : MonoBehaviour
    {
        [SerializeField] private PrimitiveType _primitiveType;

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
        }
    }
}