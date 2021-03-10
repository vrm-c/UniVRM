using UnityEngine;

namespace UniGLTF
{
    public static class EditorAnimation
    {
        public static void OnGUIAnimation(GltfParser parser)
        {
            for (int i = 0; i < parser.GLTF.animations.Count; ++i)
            {
                var a = parser.GLTF.animations[i];
                GUILayout.Label($"{i}: {a.name}");
            }
        }
    }
}
