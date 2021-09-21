

using UnityEngine;

namespace UniGLTF
{
    public static class TextureTransform
    {
        /// <summary>
        // UV Coordinate Conversion: glTF(top-left origin) to Unity(bottom-left origin)
        /// https://github.com/vrm-c/UniVRM/issues/930
        /// offset.y = 1.0f - offset.y - scale.y;
        /// </summary>
        /// <param name="s"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static (Vector2 Scale, Vector2 Offset) VerticalFlipScaleOffset(Vector2 s, Vector2 o)
        {
            return (new Vector2(s.x, s.y), new Vector2(o.x, 1.0f - o.y - s.y));
        }
    }
}
