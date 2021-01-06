using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;

namespace MeshUtility
{
    [CustomEditor(typeof(Humanoid))]
    public class HumanoidEditor : Editor
    {
        const int LABEL_WIDTH = 100;

        Humanoid m_target;

        SerializedProperty m_Hips;
        #region leg
        SerializedProperty m_LeftUpperLeg;
        SerializedProperty m_RightUpperLeg;
        SerializedProperty m_LeftLowerLeg;
        SerializedProperty m_RightLowerLeg;
        SerializedProperty m_LeftFoot;
        SerializedProperty m_RightFoot;
        SerializedProperty m_LeftToes;
        SerializedProperty m_RightToes;

        #endregion

        #region spine
        SerializedProperty m_Spine;
        SerializedProperty m_Chest;
        SerializedProperty m_UpperChest;
        SerializedProperty m_Neck;
        SerializedProperty m_Head;
        SerializedProperty m_LeftEye;
        SerializedProperty m_RightEye;
        SerializedProperty m_Jaw;

        #endregion

        #region arm
        SerializedProperty m_LeftShoulder;
        SerializedProperty m_RightShoulder;
        SerializedProperty m_LeftUpperArm;
        SerializedProperty m_RightUpperArm;
        SerializedProperty m_LeftLowerArm;
        SerializedProperty m_RightLowerArm;
        SerializedProperty m_LeftHand;
        SerializedProperty m_RightHand;

        #endregion

        #region fingers
        SerializedProperty m_LeftThumbProximal;
        SerializedProperty m_LeftThumbIntermediate;
        SerializedProperty m_LeftThumbDistal;
        SerializedProperty m_LeftIndexProximal;
        SerializedProperty m_LeftIndexIntermediate;
        SerializedProperty m_LeftIndexDistal;
        SerializedProperty m_LeftMiddleProximal;
        SerializedProperty m_LeftMiddleIntermediate;
        SerializedProperty m_LeftMiddleDistal;
        SerializedProperty m_LeftRingProximal;
        SerializedProperty m_LeftRingIntermediate;
        SerializedProperty m_LeftRingDistal;
        SerializedProperty m_LeftLittleProximal;
        SerializedProperty m_LeftLittleIntermediate;
        SerializedProperty m_LeftLittleDistal;
        SerializedProperty m_RightThumbProximal;
        SerializedProperty m_RightThumbIntermediate;
        SerializedProperty m_RightThumbDistal;
        SerializedProperty m_RightIndexProximal;
        SerializedProperty m_RightIndexIntermediate;
        SerializedProperty m_RightIndexDistal;
        SerializedProperty m_RightMiddleProximal;
        SerializedProperty m_RightMiddleIntermediate;
        SerializedProperty m_RightMiddleDistal;
        SerializedProperty m_RightRingProximal;
        SerializedProperty m_RightRingIntermediate;
        SerializedProperty m_RightRingDistal;
        SerializedProperty m_RightLittleProximal;
        SerializedProperty m_RightLittleIntermediate;
        SerializedProperty m_RightLittleDistal;

        #endregion

        void OnEnable()
        {
            m_target = target as Humanoid;
            m_Hips = serializedObject.FindProperty($"m_{nameof(Humanoid.Hips)}");

            #region legs
            m_LeftUpperLeg = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftUpperLeg)}");
            m_RightUpperLeg = serializedObject.FindProperty($"m_{nameof(Humanoid.RightUpperLeg)}");
            m_LeftLowerLeg = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftLowerLeg)}");
            m_RightLowerLeg = serializedObject.FindProperty($"m_{nameof(Humanoid.RightLowerLeg)}");
            m_LeftFoot = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftFoot)}");
            m_RightFoot = serializedObject.FindProperty($"m_{nameof(Humanoid.RightFoot)}");
            m_LeftToes = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftToes)}");
            m_RightToes = serializedObject.FindProperty($"m_{nameof(Humanoid.RightToes)}");
            #endregion

            #region spine
            m_Spine = serializedObject.FindProperty($"m_{nameof(Humanoid.Spine)}");
            m_Chest = serializedObject.FindProperty($"m_{nameof(Humanoid.Chest)}");
            m_UpperChest = serializedObject.FindProperty($"m_{nameof(Humanoid.UpperChest)}");
            m_Neck = serializedObject.FindProperty($"m_{nameof(Humanoid.Neck)}");
            m_Head = serializedObject.FindProperty($"m_{nameof(Humanoid.Head)}");
            m_LeftEye = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftEye)}");
            m_RightEye = serializedObject.FindProperty($"m_{nameof(Humanoid.RightEye)}");
            m_Jaw = serializedObject.FindProperty($"m_{nameof(Humanoid.Jaw)}");

            #endregion

            #region arm
            m_LeftShoulder = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftShoulder)}");
            m_RightShoulder = serializedObject.FindProperty($"m_{nameof(Humanoid.RightShoulder)}");
            m_LeftUpperArm = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftUpperArm)}");
            m_RightUpperArm = serializedObject.FindProperty($"m_{nameof(Humanoid.RightUpperArm)}");
            m_LeftLowerArm = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftLowerArm)}");
            m_RightLowerArm = serializedObject.FindProperty($"m_{nameof(Humanoid.RightLowerArm)}");
            m_LeftHand = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftHand)}");
            m_RightHand = serializedObject.FindProperty($"m_{nameof(Humanoid.RightHand)}");

            #endregion

            #region fingers
            m_LeftThumbProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftThumbProximal)}");
            m_LeftThumbIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftThumbIntermediate)}");
            m_LeftThumbDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftThumbDistal)}");
            m_LeftIndexProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftIndexProximal)}");
            m_LeftIndexIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftIndexIntermediate)}");
            m_LeftIndexDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftIndexDistal)}");
            m_LeftMiddleProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftMiddleProximal)}");
            m_LeftMiddleIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftMiddleIntermediate)}");
            m_LeftMiddleDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftMiddleDistal)}");
            m_LeftRingProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftRingProximal)}");
            m_LeftRingIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftRingIntermediate)}");
            m_LeftRingDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftRingDistal)}");
            m_LeftLittleProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftLittleProximal)}");
            m_LeftLittleIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftLittleIntermediate)}");
            m_LeftLittleDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.LeftLittleDistal)}");
            m_RightThumbProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightThumbProximal)}");
            m_RightThumbIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.RightThumbIntermediate)}");
            m_RightThumbDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightThumbDistal)}");
            m_RightIndexProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightIndexProximal)}");
            m_RightIndexIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.RightIndexIntermediate)}");
            m_RightIndexDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightIndexDistal)}");
            m_RightMiddleProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightMiddleProximal)}");
            m_RightMiddleIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.RightMiddleIntermediate)}");
            m_RightMiddleDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightMiddleDistal)}");
            m_RightRingProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightRingProximal)}");
            m_RightRingIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.RightRingIntermediate)}");
            m_RightRingDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightRingDistal)}");
            m_RightLittleProximal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightLittleProximal)}");
            m_RightLittleIntermediate = serializedObject.FindProperty($"m_{nameof(Humanoid.RightLittleIntermediate)}");
            m_RightLittleDistal = serializedObject.FindProperty($"m_{nameof(Humanoid.RightLittleDistal)}");
            #endregion
        }

        struct Horizontal : IDisposable
        {
            public static Horizontal Using()
            {
                EditorGUILayout.BeginHorizontal();
                return default;
            }
            public void Dispose()
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        static void HorizontalFields(string label, params SerializedProperty[] props)
        {
            try
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(label, GUILayout.Width(LABEL_WIDTH));
                GUILayout.FlexibleSpace();

                foreach (var prop in props)
                {
                    EditorGUILayout.PropertyField(prop, GUIContent.none, true, GUILayout.MinWidth(100));
                }
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        static bool s_spineFold;
        static bool s_legFold;
        static bool s_armFold;
        static bool s_fingerFold;
        static string GetDialogDir(UnityEngine.Object obj)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (prefab == null)
            {
                return null;
            }
            return UnityPath.FromAsset(prefab).FullPath;
        }

        public override void OnInspectorGUI()
        {
            foreach (var validation in m_target.Validate())
            {
                EditorGUILayout.HelpBox(validation.Message, validation.IsError ? MessageType.Error : MessageType.Warning);
            }

            // prefer
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Hips);

            s_spineFold = EditorGUILayout.Foldout(s_spineFold, "Body");
            if (s_spineFold)
            {
                EditorGUILayout.PropertyField(m_Spine);
                EditorGUILayout.PropertyField(m_Chest);
                EditorGUILayout.PropertyField(m_UpperChest);
                EditorGUILayout.PropertyField(m_Neck);
                EditorGUILayout.PropertyField(m_Head);
                EditorGUILayout.PropertyField(m_Jaw);
                HorizontalFields("Eye", m_LeftEye, m_RightEye);
            }

            s_legFold = EditorGUILayout.Foldout(s_legFold, "Leg");
            if (s_legFold)
            {
                HorizontalFields("UpperLeg", m_LeftUpperLeg, m_RightUpperLeg);
                HorizontalFields("LowerLeg", m_LeftLowerLeg, m_RightLowerLeg);
                HorizontalFields("Foot", m_LeftFoot, m_RightFoot);
                HorizontalFields("Toes", m_LeftToes, m_RightToes);
            }

            s_armFold = EditorGUILayout.Foldout(s_armFold, "Arm");
            if (s_armFold)
            {
                HorizontalFields("Shoulder", m_LeftShoulder, m_RightShoulder);
                HorizontalFields("UpperArm", m_LeftUpperArm, m_RightUpperArm);
                HorizontalFields("LowerArm", m_LeftLowerArm, m_RightLowerArm);
                HorizontalFields("Hand", m_LeftHand, m_RightHand);
            }

            s_fingerFold = EditorGUILayout.Foldout(s_fingerFold, "Finger");
            if (s_fingerFold)
            {
                HorizontalFields("LeftThumb", m_LeftThumbProximal, m_LeftThumbIntermediate, m_LeftThumbDistal);
                HorizontalFields("LeftIndex", m_LeftIndexProximal, m_LeftIndexIntermediate, m_LeftIndexDistal);
                HorizontalFields("LeftMiddle", m_LeftMiddleProximal, m_LeftMiddleIntermediate, m_LeftMiddleDistal);
                HorizontalFields("LeftRing", m_LeftRingProximal, m_LeftRingIntermediate, m_LeftRingDistal);
                HorizontalFields("LeftLittle", m_LeftLittleProximal, m_LeftLittleIntermediate, m_LeftLittleDistal);
                HorizontalFields("RightThumb", m_RightThumbProximal, m_RightThumbIntermediate, m_RightThumbDistal);
                HorizontalFields("RightIndex", m_RightIndexProximal, m_RightIndexIntermediate, m_RightIndexDistal);
                HorizontalFields("RightMiddle", m_RightMiddleProximal, m_RightMiddleIntermediate, m_RightMiddleDistal);
                HorizontalFields("RightRing", m_RightRingProximal, m_RightRingIntermediate, m_RightRingDistal);
                HorizontalFields("RightLittle", m_RightLittleProximal, m_RightLittleIntermediate, m_RightLittleDistal);
            }

            serializedObject.ApplyModifiedProperties();

            // create avatar
            if (GUILayout.Button("Create UnityEngine.Avatar"))
            {
                var path = EditorUtility.SaveFilePanel(
                        "Save avatar",
                        GetDialogDir(m_target),
                        string.Format("{0}.avatar.asset", serializedObject.targetObject.name),
                        "asset");
                if (!string.IsNullOrEmpty(path))
                {
                    var avatar = m_target.CreateAvatar();
                    if (avatar != null)
                    {
                        var unityPath = UnityPath.FromFullpath(path);
                        avatar.name = "avatar";
                        Debug.LogFormat("Create avatar {0}", unityPath);
                        AssetDatabase.CreateAsset(avatar, unityPath.Value);
                        AssetDatabase.ImportAsset(unityPath.Value);

                        // replace
                        var animator = m_target.GetComponent<Animator>();
                        if (animator == null)
                        {
                            animator = m_target.gameObject.AddComponent<Animator>();
                        }
                        animator.avatar = avatar;

                        Selection.activeObject = avatar;
                    }
                }
            }
        }

        void OnSceneGUI()
        {
            // var bones = m_target.Bones;
            // if (bones != null)
            // {
            //     for (int i = 0; i < bones.Length; ++i)
            //     {
            //         DrawBone((HumanBodyBones)i, bones[i]);
            //     }
            //     foreach (var x in m_bones)
            //     {
            //         x.Draw();
            //     }
            // }

            var forward = m_target.GetForward();

            var begin = m_target.transform.position;
            var end = begin + forward;
            Handles.DrawLine(begin, end);
        }
    }
}
