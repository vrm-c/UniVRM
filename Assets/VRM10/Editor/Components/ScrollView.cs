using System;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    public class ScrollView
    {
        Vector2 m_scrollPosition;

        public void Draw(float height, Action content, Action repaint)
        {
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            // mouse wheel scroll part 1
            var isScroll = Event.current.isScrollWheel;
            if (isScroll)
            {
                m_scrollPosition += Event.current.delta * EditorGUIUtility.singleLineHeight;
                if (m_scrollPosition.y < 0)
                {
                    m_scrollPosition = Vector2.zero;
                }
            }

            content();

            // mouse wheel scroll part 2
            var bottom = EditorGUILayout.GetControlRect();
            if (isScroll)
            {
                var maxScroll = bottom.y - (height - EditorGUIUtility.singleLineHeight * 2);
                // Debug.Log($"{bottom.y}: {this.position.size.y}: {maxScroll}");
                if (m_scrollPosition.y > maxScroll)
                {
                    m_scrollPosition = new Vector2(0, maxScroll);
                }
                repaint();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
