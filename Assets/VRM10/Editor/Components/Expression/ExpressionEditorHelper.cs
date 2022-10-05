using System;
using UnityEditor;
using UnityEngine;


namespace UniVRM10
{
    public static class ExpressionEditorHelper
    {
        public static bool StringPopup(Rect rect, SerializedProperty prop, string[] options, out int newIndex)
        {
            if (options == null)
            {
                newIndex = -1;
                return false;
            }

            var oldIndex = Array.IndexOf(options, prop.stringValue);
            newIndex = EditorGUI.Popup(rect, oldIndex, options);
            if (newIndex != oldIndex && newIndex >= 0 && newIndex < options.Length)
            {
                prop.stringValue = options[newIndex];
                return true;
            }
            else
            {
                return false;
            }
        }

        public static T EnumPopup<T>(Rect rect, T value) where T : Enum
        {
            return (T)EditorGUI.EnumPopup(rect, (Enum)value);
        }

        public static bool IntPopup(Rect rect, SerializedProperty prop, string[] options, out int newIndex)
        {
            if (options == null)
            {
                newIndex = -1;
                return false;
            }

            var oldIndex = prop.intValue;
            newIndex = EditorGUI.Popup(rect, oldIndex, options);
            if (newIndex != oldIndex && newIndex >= 0 && newIndex < options.Length)
            {
                prop.intValue = newIndex;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool FloatSlider(Rect rect, SerializedProperty prop, float maxValue)
        {
            var oldValue = prop.floatValue;
            var newValue = EditorGUI.Slider(rect, prop.floatValue, 0, maxValue);
            if (newValue != oldValue)
            {
                prop.floatValue = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ColorProp(Rect rect, SerializedProperty prop)
        {
            var oldValue = (Color)prop.vector4Value;
            var newValue = EditorGUI.ColorField(rect, prop.displayName, oldValue);
            if (newValue != oldValue)
            {
                prop.vector4Value = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UVProp(Rect rect, SerializedProperty prop)
        {
            var oldValue = prop.vector2Value;
            var newValue = EditorGUI.Vector2Field(rect, prop.displayName, oldValue);
            if (newValue != oldValue)
            {
                prop.vector2Value = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }

        static Rect AdvanceRect(ref float x, float y, float w, float h)
        {
            var rect = new Rect(x, y, w, h);
            x += w;
            return rect;
        }

        static float[] v2 = new float[2];
        static GUIContent[] l2 = new GUIContent[]{
            new GUIContent("x"),
            new GUIContent("y")
        };
        static Vector4 TilingOffset(Rect rect, string label, Vector4 src)
        {
            /*
            var style = new GUIStyle()
            {
                alignment = TextAnchor.MiddleRight,
            };
            */

            var quad = (rect.width - 56);
            var x = rect.x;
            //EditorGUIUtility.labelWidth = 18;

            EditorGUI.LabelField(AdvanceRect(ref x, rect.y, 40, rect.height), "Tiling");
            v2[0] = src.x;
            v2[1] = src.y;
            EditorGUI.MultiFloatField(AdvanceRect(ref x, rect.y, quad, rect.height), l2, v2);
            src.x = v2[0];
            src.y = v2[1];

            //EditorGUI.LabelField(AdvanceRect(ref x, rect.y, quad, rect.height), "Y", style);
            //src.y = EditorGUI.FloatField(AdvanceRect(ref x, rect.y, quad, rect.height), "Y", src.y);

            rect.y += EditorGUIUtility.singleLineHeight;
            x = rect.x;
            EditorGUI.LabelField(AdvanceRect(ref x, rect.y, 40, rect.height), "Offset");
            v2[0] = src.z;
            v2[1] = src.w;
            EditorGUI.MultiFloatField(AdvanceRect(ref x, rect.y, quad, rect.height), l2, v2);
            src.z = v2[0];
            src.w = v2[1];

            //EditorGUI.LabelField(AdvanceRect(ref x, rect.y, quad * 2, rect.height), "Offset X", style);
            //src.z = EditorGUI.FloatField(AdvanceRect(ref x, rect.y, quad, rect.height), "X", src.z);

            //EditorGUI.LabelField(AdvanceRect(ref x, rect.y, quad, rect.height), "Y", style);
            //src.w = EditorGUI.FloatField(AdvanceRect(ref x, rect.y, quad, rect.height), "Y", src.w);

            return src;
        }

        static bool OffsetProp(Rect rect, SerializedProperty prop)
        {
            var oldValue = prop.vector4Value;
            //var newValue = EditorGUI.Vector4Field(rect, prop.displayName, oldValue);
            var newValue = TilingOffset(rect, prop.displayName, oldValue);
            if (newValue != oldValue)
            {
                prop.vector4Value = newValue;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// https://gist.github.com/gszauer/7799899
    public class TextureScale
    {
        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;

        public static void Scale(Texture2D tex, int newWidth, int newHeight)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
            ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            w = tex.width;
            w2 = newWidth;

            BilinearScale(0, newHeight);

#if UNITY_2021_2_OR_NEWER
            tex.Reinitialize(newWidth, newHeight);
#else
            tex.Resize(newWidth, newHeight);
#endif
            tex.SetPixels(newColors);
            tex.Apply();
        }

        private static void BilinearScale(int start, int end)
        {
            for (var y = start; y < end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }

        /// http://light11.hatenadiary.com/entry/2018/04/19/194015
        public static Texture2D GetResized(Texture2D texture, int width, int height)
        {
            // リサイズ後のサイズを持つRenderTextureを作成して書き込む
            var rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(texture, rt);

            // リサイズ後のサイズを持つTexture2Dを作成してRenderTextureから書き込む
            var preRT = RenderTexture.active;
            RenderTexture.active = rt;
            var ret = new Texture2D(width, height);
            ret.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            ret.Apply();
            RenderTexture.active = preRT;

            RenderTexture.ReleaseTemporary(rt);
            return ret;
        }
    }
}
