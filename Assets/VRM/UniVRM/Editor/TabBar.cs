using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRM
{

    static class TabBar
    {
        public static T OnGUI<T>(T t, GUIStyle buttonStyle, GUI.ToolbarButtonSize buttonSize) where T : Enum
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                // タブを描画する
                var value = GUILayout.Toolbar((int)(object)t, TabCache<T>.TabToggles, buttonStyle, buttonSize);
                GUILayout.FlexibleSpace();
                return (T)(object)value;
            }
        }
        static class TabCache<T> where T : Enum
        {
            private static GUIContent[] _tabToggles = null;

            public static GUIContent[] TabToggles
            {
                get
                {
                    if (_tabToggles == null)
                    {
                        _tabToggles = System.Enum.GetNames(typeof(T)).Select(x => new GUIContent(x)).ToArray();
                    }
                    return _tabToggles;
                }
            }
        }
    }
}
