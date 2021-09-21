using UnityEngine;

namespace UniGLTF.Samples
{
    /// <summary>
    /// ScaleOffset の検証
    /// </summary>
    public class ScaleOffset : MonoBehaviour
    {
        [SerializeField]
        public Vector2 Scale = Vector2.one;

        [SerializeField]
        public Vector2 Offset = Vector2.zero;

        [SerializeField]
        Material Unity;

        [SerializeField]
        Material Gltf;

        void OnValidate()
        {
            Execute();
        }

        // Update is called once per frame
        void Update()
        {
            Execute();
        }

        const string PROP_NAME = "_MainTex";

        void Execute()
        {
            if (Unity == null || Gltf == null)
            {
                return;
            }

            Unity.SetTextureScale(PROP_NAME, Scale);
            Unity.SetTextureOffset(PROP_NAME, Offset);

            var (s, o) = TextureTransform.VerticalFlipScaleOffset(Scale, Offset);

            Gltf.SetTextureScale(PROP_NAME, s);
            Gltf.SetTextureOffset(PROP_NAME, o);
        }
    }
}