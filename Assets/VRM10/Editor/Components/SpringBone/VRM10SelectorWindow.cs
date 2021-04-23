using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// ヒエラルキーから VRM10SpringBone と VRM10ColliderGroup を集めてリスト表示する
    /// </summary>
    public class VRM10SelectorWindow : EditorWindow
    {
        const string MENU_KEY = VRMVersion.MENU + "/VRM selector window";
        const string WINDOW_TITLE = "VRM selector";

        [MenuItem(MENU_KEY, false, 1)]
        private static void ExportFromMenu()
        {
            var window = (VRM10SelectorWindow)GetWindow(typeof(VRM10SelectorWindow));
            window.titleContent = new GUIContent(WINDOW_TITLE);
            window.Show();
            window.Root = Selection.activeTransform;
        }

        void OnEnable()
        {
            // Debug.Log("OnEnable");
            Undo.willFlushUndoRecord += Repaint;
            Selection.selectionChanged += Repaint;
        }

        void OnDisable()
        {
            // Debug.Log("OnDisable");
            Selection.selectionChanged -= Repaint;
            Undo.willFlushUndoRecord -= Repaint;
        }

        Transform m_root;
        Transform Root
        {
            get => m_root;
            set
            {
                if (m_root == value)
                {
                    return;
                }
                if (value != null && !value.gameObject.scene.IsValid())
                {
                    // skip prefab
                    return;
                }
                m_root = value;

                m_springs = null;
                m_colliderGroups = null;
                m_boneMap = null;
            }
        }

        static bool s_foldHumanoidBones = true;
        static HumanBodyBones[] s_bones;
        public Dictionary<HumanBodyBones, Transform> m_boneMap;

        static bool s_foldSprings = true;
        public VRM10SpringBone[] m_springs;

        static bool s_foldColliders = true;
        public VRM10SpringBoneColliderGroup[] m_colliderGroups;

        void Reload()
        {
            var backup = Root;
            Root = null;
            Root = backup;
        }

        Vector2 m_scrollPosition;

        private void OnGUI()
        {
            Root = (Transform)EditorGUILayout.ObjectField("vrm1 root", m_root, typeof(Transform), true);

            if (m_springs != null && m_springs.Length > 0 && m_springs[0] == null)
            {
                Reload();
            }

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);

            var isScroll = Event.current.isScrollWheel;
            if (isScroll)
            {
                m_scrollPosition += Event.current.delta * EditorGUIUtility.singleLineHeight;
                if (m_scrollPosition.y < 0)
                {
                    m_scrollPosition = Vector2.zero;
                }
            }
            DrawContent();
            var bottom = EditorGUILayout.GetControlRect();
            if (isScroll)
            {
                var maxScroll = bottom.y - (this.position.size.y - EditorGUIUtility.singleLineHeight * 2);
                // Debug.Log($"{bottom.y}: {this.position.size.y}: {maxScroll}");
                if (m_scrollPosition.y > maxScroll)
                {
                    m_scrollPosition = new Vector2(0, maxScroll);
                }
                Repaint();
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawContent()
        {
            if (m_root != null)
            {
                if (s_bones == null)
                {
                    var values = Enum.GetValues(typeof(HumanBodyBones));
                    s_bones = new HumanBodyBones[values.Length - 1];
                    int j = 0;
                    foreach (HumanBodyBones bone in values)
                    {
                        if (bone != HumanBodyBones.LastBone)
                        {
                            s_bones[j++] = bone;
                        }
                    }
                }
                if (m_boneMap == null)
                {
                    var animator = m_root.GetComponent<Animator>();
                    if (animator != null)
                    {
                        m_boneMap = new Dictionary<HumanBodyBones, Transform>();
                        foreach (var bone in s_bones)
                        {
                            var t = animator.GetBoneTransform(bone);
                            if (t != null)
                            {
                                m_boneMap.Add(bone, t);
                            }
                        }
                    }
                }
                if (m_springs == null)
                {
                    m_springs = m_root.GetComponentsInChildren<VRM10SpringBone>();
                }
                if (m_colliderGroups == null)
                {
                    m_colliderGroups = m_root.GetComponentsInChildren<VRM10SpringBoneColliderGroup>();
                }
            }

            GUI.enabled = false;
            s_foldHumanoidBones = EditorGUILayout.Foldout(s_foldHumanoidBones, "human bones");
            if (s_foldHumanoidBones)
            {
                if (m_boneMap != null)
                {
                    foreach (var bone in s_bones)
                    {
                        if (m_boneMap.TryGetValue(bone, out Transform t))
                        {

                        }
                        EditorGUILayout.ObjectField(bone.ToString(), t, typeof(Transform), true);
                    }
                }
            }

            s_foldSprings = EditorGUILayout.Foldout(s_foldSprings, "springs");
            if (s_foldSprings)
            {
                if (m_springs != null)
                {
                    foreach (var s in m_springs)
                    {
                        EditorGUILayout.ObjectField(s, typeof(VRM10SpringBone), true);
                    }
                }
            }

            s_foldColliders = EditorGUILayout.Foldout(s_foldColliders, "colliders");
            if (s_foldColliders)
            {
                if (m_colliderGroups != null)
                {
                    foreach (var c in m_colliderGroups)
                    {
                        EditorGUILayout.ObjectField(c, typeof(VRM10SpringBoneColliderGroup), true);
                    }
                }
            }
        }
    }
}
