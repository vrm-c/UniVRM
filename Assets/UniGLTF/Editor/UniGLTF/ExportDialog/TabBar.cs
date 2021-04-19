using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MeshUtility
{

    public static class TabBar
    {
        /// <summary>
        /// GUI.ToolbarButtonSize.FitToContentsも設定できる
        /// </summary>
        /// <param name="t"></param>
        /// <param name="buttonStyle"></param>
        /// <param name="buttonSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T OnGUI<T>(T t, GUIStyle buttonStyle = null, GUI.ToolbarButtonSize buttonSize = GUI.ToolbarButtonSize.Fixed) where T : Enum
        {
            if (buttonStyle == null)
            {
                buttonStyle = "LargeButton";
            }
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
