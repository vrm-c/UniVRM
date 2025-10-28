using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace VRM
{

    [CustomPropertyDrawer(typeof(BlendShapeClip))]
    public class BlendShapeClipDrawer : PropertyDrawer
    {
        //public const int Height = 132;

        //public const int ThumbnailSize = 128;

        public const int Height = 80;
        public const int ThumbnailSize = 0;

        public override void OnGUI(Rect position,
          SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                //EditorGUIUtility.labelWidth = 80;

                position.height = EditorGUIUtility.singleLineHeight;

                //var halfWidth = position.width * 0.5f;

                var rect = new Rect(position.x + ThumbnailSize, position.y, position.width - ThumbnailSize, position.height);
                EditorGUI.PropertyField(rect, property);

                var clip = property.objectReferenceValue as BlendShapeClip;
                if (clip != null)
                {
                    var clipObj = new SerializedObject(clip);
                    //var thumbnail = clipObj.FindProperty("Thumbnail");
                    var blendShapeName = clipObj.FindProperty("BlendShapeName");
                    var preset = clipObj.FindProperty("Preset");
                    var isBinary = clipObj.FindProperty("IsBinary");

                    /*
                    EditorGUI.ObjectField(new Rect(position)
                    {
                        width = ThumbnailSize,
                        height = ThumbnailSize
                    }, thumbnail.objectReferenceValue, typeof(Texture), false);
                    */

                    rect.y += (EditorGUIUtility.singleLineHeight + 2);
                    EditorGUI.PropertyField(rect, blendShapeName);
                    rect.y += (EditorGUIUtility.singleLineHeight + 2);
                    EditorGUI.PropertyField(rect, preset);
                    rect.y += (EditorGUIUtility.singleLineHeight + 2);
                    EditorGUI.PropertyField(rect, isBinary);

                    clipObj.ApplyModifiedProperties();
                }
            }
        }
    }
}