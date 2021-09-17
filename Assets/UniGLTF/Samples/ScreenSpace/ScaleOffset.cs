using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// https://github.com/vrm-c/UniVRM/issues/930
        /// </summary>
        /// <param name="s"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static (Vector2, Vector2) ConvScaleOffset(Vector2 s, Vector2 o)
        {
            return (new Vector2(s.x, s.y), new Vector2(o.x, 1 - o.y - s.y));
        }

        void Execute()
        {
            Unity.SetTextureScale(PROP_NAME, Scale);
            Unity.SetTextureOffset(PROP_NAME, Offset);

            var (s, o) = ConvScaleOffset(Scale, Offset);

            Gltf.SetTextureScale(PROP_NAME, s);
            Gltf.SetTextureOffset(PROP_NAME, o);
        }
    }
}