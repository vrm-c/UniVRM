using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniGLTF;

namespace UniVRM10
{
    using static Vrm10InstanceSpringBone;

    [CustomPropertyDrawer(typeof(Spring))]
    public class VRM10SpringBoneDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3
                + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Spring.ColliderGroups)))
                + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Spring.Joints)))
                ;
        }

        static float DrawProp(Rect rect, SerializedProperty prop)
        {
            rect.height = EditorGUI.GetPropertyHeight(prop);
            EditorGUI.PropertyField(rect, prop);
            return rect.height;
        }

        struct GuiEnable : IDisposable
        {
            bool Backup;
            public static GuiEnable Enter(bool enable)
            {
                var value = new GuiEnable
                {
                    Backup = GUI.enabled,
                };
                GUI.enabled = enable;
                return value;
            }

            public void Dispose()
            {
                GUI.enabled = Backup;
            }
        }

        static IEnumerable<VRM10SpringBoneJoint> MakeJointsRecursive(VRM10SpringBoneJoint parent)
        {
            if (parent.transform.childCount > 0)
            {
                var child = parent.transform.GetChild(0);
                var joint = child.gameObject.GetOrAddComponent<VRM10SpringBoneJoint>();
                // set params
                joint.m_dragForce = parent.m_dragForce;
                joint.m_gravityDir = parent.m_gravityDir;
                joint.m_gravityPower = parent.m_gravityPower;
                joint.m_jointRadius = parent.m_jointRadius;
                joint.m_stiffnessForce = parent.m_stiffnessForce;

                yield return joint;
                foreach (var x in MakeJointsRecursive(joint))
                {
                    yield return x;
                }
            }
        }

        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var x = rect.x;
            var y = rect.y;
            var w = rect.width;
            var h = EditorGUIUtility.singleLineHeight;
            y += DrawProp(new Rect(x, y, w, h), property.FindPropertyRelative(nameof(Spring.Name)));
            y += DrawProp(new Rect(x, y, w, h), property.FindPropertyRelative(nameof(Spring.ColliderGroups)));

            var joints = property.FindPropertyRelative(nameof(Spring.Joints));
            y += DrawProp(new Rect(x, y, w, h), joints);

            var enable = joints.arraySize > 0 && joints.GetArrayElementAtIndex(0).objectReferenceValue != null;
            using (GuiEnable.Enter(enable))
            {
                if (GUI.Button(new Rect(x, y, w, h), "create joints to children"))
                {
                    if (EditorUtility.DisplayDialog("auto joints",
                        "先頭の joint の子孫をリストに追加します。\n既存のリストは上書きされます。",
                        "ok",
                        "cancel"))
                    {
                        var root = (VRM10SpringBoneJoint)joints.GetArrayElementAtIndex(0).objectReferenceValue;
                        joints.ClearArray();
                        int i = 0;
                        // 0
                        joints.InsertArrayElementAtIndex(i);
                        joints.GetArrayElementAtIndex(i).objectReferenceValue = root;
                        ++i;
                        // 1...
                        foreach (var joint in MakeJointsRecursive(root))
                        {
                            joints.InsertArrayElementAtIndex(i);
                            joints.GetArrayElementAtIndex(i).objectReferenceValue = joint;
                            ++i;
                        }
                    }
                }
            }
            y += EditorGUIUtility.singleLineHeight;

            y += DrawProp(new Rect(x, y, w, h), property.FindPropertyRelative(nameof(Spring.Center)));
        }
    }
}