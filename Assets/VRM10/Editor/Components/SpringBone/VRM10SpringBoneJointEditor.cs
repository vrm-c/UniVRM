using System;
using UnityEditor;
using UnityEngine;

namespace UniVRM10
{
    [CustomEditor(typeof(VRM10SpringBoneJoint))]
    [CanEditMultipleObjects]
    class VRM10SpringBoneJointEditor : Editor
    {
        private VRM10SpringBoneJoint m_target;

        private SerializedProperty m_script;
        private SerializedProperty m_stiffnessForceProp;
        private SerializedProperty m_gravityPowerProp;
        private SerializedProperty m_gravityDirProp;
        private SerializedProperty m_dragForceProp;
        private SerializedProperty m_jointRadiusProp;
        private SerializedProperty m_angleLimitType;
        private SerializedProperty m_angleLimitRotation;
        private SerializedProperty m_angleLimitAngle1;
        private SerializedProperty m_angleLimitAngle2;

        private Vrm10Instance m_root;

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            m_target = (VRM10SpringBoneJoint)target;

            m_script = serializedObject.FindProperty("m_Script");
            m_stiffnessForceProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_stiffnessForce));
            m_gravityPowerProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_gravityPower));
            m_gravityDirProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_gravityDir));
            m_dragForceProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_dragForce));
            m_jointRadiusProp = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_jointRadius));
            m_angleLimitType = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_anglelimitType));
            m_angleLimitRotation = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_angleLimitRotation));
            m_angleLimitAngle1 = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_angleLimitAngle1));
            m_angleLimitAngle2 = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_angleLimitAngle2));

            m_root = m_target.GetComponentInParent<Vrm10Instance>();
        }

        static bool m_showJoints;
        static bool m_showColliders;
        static bool m_showJointSettings;
        static bool m_showAnglelimitSettings;

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(m_script);
            }

            serializedObject.Update();

            ///
            /// spring 情報。readonly。mouse click による hierarchy 参照補助
            /// 
            EditorGUI.BeginDisabledGroup(true);
            var isLastTail = ShowSpringInfo();
            EditorGUI.EndDisabledGroup();

            if (isLastTail)
            {
                EditorGUILayout.HelpBox("末端 joint です。下記の設定は使用されません。", MessageType.Info);
            }

            //
            // joint
            //
            m_showJointSettings = EditorGUILayout.Foldout(m_showJointSettings, "Joint Settings");
            if (m_showJointSettings)
            {
                Vrm10EditorUtility.LimitBreakSlider(m_stiffnessForceProp, 0.0f, 4.0f, 0.0f, Mathf.Infinity);
                Vrm10EditorUtility.LimitBreakSlider(m_gravityPowerProp, 0.0f, 2.0f, 0.0f, Mathf.Infinity);
                EditorGUILayout.PropertyField(m_gravityDirProp);
                EditorGUILayout.PropertyField(m_dragForceProp);
                EditorGUILayout.Space();
                Vrm10EditorUtility.LimitBreakSlider(m_jointRadiusProp, 0.0f, 0.5f, 0.0f, Mathf.Infinity);
            }

            //
            // angle limit
            //
            m_showAnglelimitSettings = EditorGUILayout.Foldout(m_showAnglelimitSettings, "AngleLimit Settings (experimental)");
            if (m_showAnglelimitSettings)
            {
                EditorGUILayout.HelpBox("SpringBoneの角度制限はまだdraft仕様です。将来的に仕様が変更される可能性があります。また、VRMファイルへのインポート・エクスポート機能はまだ実装されていません。\nThe angle limit feature for SpringBone is still in draft status. The specifications may change in the future. Also, the import/export of VRM files has not yet been implemented.", MessageType.Warning);

                EditorGUILayout.PropertyField(m_angleLimitType);
                switch ((UniGLTF.SpringBoneJobs.AnglelimitTypes)m_angleLimitType.enumValueIndex)
                {
                    case UniGLTF.SpringBoneJobs.AnglelimitTypes.None:
                        break;

                    case UniGLTF.SpringBoneJobs.AnglelimitTypes.Cone:
                        EditorGUILayout.PropertyField(m_angleLimitRotation);
                        EditorGUILayout.PropertyField(m_angleLimitAngle1);
                        break;

                    case UniGLTF.SpringBoneJobs.AnglelimitTypes.Hinge:
                        EditorGUILayout.PropertyField(m_angleLimitRotation);
                        EditorGUILayout.PropertyField(m_angleLimitAngle1);
                        break;

                    case UniGLTF.SpringBoneJobs.AnglelimitTypes.Spherical:
                        EditorGUILayout.PropertyField(m_angleLimitRotation);
                        EditorGUILayout.PropertyField(m_angleLimitAngle1);
                        EditorGUILayout.PropertyField(m_angleLimitAngle2);
                        break;
                }
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                if (Application.isPlaying)
                {
                    if (m_root != null)
                    {
                        m_root.Runtime.SpringBone.SetJointLevel(m_target.transform, m_target.Blittable);
                    }
                }
            }
        }

        /// <summary>
        /// - Jointの所属するSpringの情報を表示する
        /// - Springに関連付けられたColliderの情報を表示する
        /// </summary>
        /// <returns>末端</returns>
        bool ShowSpringInfo()
        {
            if (m_root == null)
            {
                EditorGUILayout.HelpBox("no vrm-1.0", MessageType.Warning);
                return false;
            }
            EditorGUILayout.ObjectField("Vrm-1.0", m_root, typeof(Vrm10Instance), true, null);

            var found = m_root.SpringBone.FindJoint(m_target);
            if (!found.HasValue)
            {
                EditorGUILayout.HelpBox("This joint not belongs any spring", MessageType.Warning);
                return false;
            }

            var (spring, i, _) = found.Value;
            m_showJoints = EditorGUILayout.Foldout(m_showJoints, $"Springs[{i}]({spring.Name})");
            int? jointIndex = default;
            // joints
            for (int j = 0; j < spring.Joints.Count; ++j)
            {
                var joint = spring.Joints[j];
                if (m_showJoints)
                {
                    var label = $"Joints[{j}]";
                    if (joint == m_target)
                    {
                        label += "★";
                    }
                    EditorGUILayout.ObjectField(label, joint, typeof(VRM10SpringBoneJoint), true, null);
                }

                if (joint == m_target)
                {
                    jointIndex = j;
                }
            }

            m_showColliders = EditorGUILayout.Foldout(m_showColliders, "ColliderGroups");
            if (m_showColliders && found.HasValue)
            {
                // collider groups
                for (int j = 0; j < spring.ColliderGroups.Count; ++j)
                {
                    var group = spring.ColliderGroups[j];
                    EditorGUILayout.LabelField($"ColliderGroups[{j}]({group.Name})");
                    for (int k = 0; k < group.Colliders.Count; ++k)
                    {
                        var collider = group.Colliders[k];
                        var label = $"Colliders[{k}]";
                        EditorGUILayout.ObjectField(label, collider, typeof(VRM10SpringBoneCollider), true, null);
                    }
                }
            }

            return jointIndex == (spring.Joints.Count - 1);
        }

        /// <summary>
        /// (0,1,0) 
        /// ^ /
        /// |/
        /// +-->
        /// 初期姿勢(T-Pose)の tail position が 0,1,0 になるように
        /// joint local 空間を補正した空間である。回転 offset として angleLimitRotation(default は identity) も乗算する。
        /// </summary>
        /// <returns></returns>
        static Quaternion calcSpringboneLimitSpace(Quaternion head, Vector3 boneAxis)
        {
            var jointLocalAxisSpace = Quaternion.FromToRotation(Vector3.up, boneAxis);
            return head * jointLocalAxisSpace;
        }

        bool HandleLimitRotation(Matrix4x4 limitSpace)
        {
            Handles.matrix = limitSpace;
            EditorGUI.BeginChangeCheck();
            Quaternion rot = Handles.RotationHandle(m_target.m_angleLimitRotation, Vector3.zero);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "m_angleLimitRotation");
                // serializedObject.Update();
                // m_angleLimitRotation.quaternionValue = rot;
                // serializedObject.ApplyModifiedProperties();
                m_target.m_angleLimitRotation = rot;
                return true;
            }
            else
            {
                return false;
            }
        }

        void OnSceneGUI()
        {
            if (m_root == null)
            {
                return;
            }

            // joint(m_target) から、所属する spring を検索する。
            var found = m_root.SpringBone.FindJoint(m_target);
            if (!found.HasValue)
            {
                return;
            }
            // 所属 spring と joint(m_target) の index を得る
            var (spring, i, j) = found.Value;

            var head = spring.Joints[j].transform;
            var label = string.IsNullOrEmpty(spring.Name)
                ? $"[{i}][{j}]{m_target.name}"
                : $"[{i}]{spring.Name}[{j}]{m_target.name}";
            Handles.Label(head.transform.position, label);

            if (j + 1 < spring.Joints.Count)
            {
                var _tail = spring.Joints[j + 1];
                if (_tail != null)
                {
                    var tail = _tail.transform;
                    var local_axis = head.worldToLocalMatrix.MultiplyPoint(tail.position);
                    var limit_tail_pos = Vector3.up * local_axis.magnitude;
                    var limitRotation = calcSpringboneLimitSpace(head.rotation, local_axis);

                    if (m_target.m_anglelimitType == UniGLTF.SpringBoneJobs.AnglelimitTypes.None)
                    {
                        Tools.hidden = false;
                    }
                    else
                    {
                        Tools.hidden = true;
                        if (HandleLimitRotation(Matrix4x4.TRS(head.position, limitRotation, Vector3.one)))
                        {
                            if (Application.isPlaying)
                            {
                                if (m_root != null)
                                {
                                    m_root.Runtime.SpringBone.SetJointLevel(m_target.transform, m_target.Blittable);
                                }
                            }
                        }
                    }

                    limitRotation = limitRotation * m_target.m_angleLimitRotation;

                    var limitSpace = Matrix4x4.TRS(head.position, limitRotation, Vector3.one);
                    Handles.matrix = limitSpace;
                    Handles.color = Color.red;
                    Handles.DrawLine(Vector3.zero, Vector3.right * limit_tail_pos.magnitude);
                    Handles.color = Color.green;
                    Handles.DrawLine(Vector3.zero, Vector3.up * limit_tail_pos.magnitude);

                    var s = Mathf.Sin(m_target.m_angleLimitAngle1 * 0.5f);
                    var c = Mathf.Cos(m_target.m_angleLimitAngle1 * 0.5f);

                    switch (m_target.m_anglelimitType)
                    {
                        case UniGLTF.SpringBoneJobs.AnglelimitTypes.Cone:
                            {
                                var r = Mathf.Tan(m_target.m_angleLimitAngle1 * 0.5f) * limit_tail_pos.magnitude * c;

                                Handles.color = Color.cyan;
                                Handles.DrawWireDisc(limit_tail_pos * c, Vector3.up, r, 1);
                                //         o head
                                //      r /
                                //      |/
                                // -r --+-- r
                                //      |
                                //      -r
                                Handles.DrawLine(Vector3.zero, new Vector3(0, c, s) * limit_tail_pos.magnitude);
                                Handles.DrawLine(Vector3.zero, new Vector3(0, c, -s) * limit_tail_pos.magnitude);
                                Handles.DrawLine(Vector3.zero, new Vector3(s, c, 0) * limit_tail_pos.magnitude);
                                Handles.DrawLine(Vector3.zero, new Vector3(-s, c, 0) * limit_tail_pos.magnitude);
                                break;
                            }

                        case UniGLTF.SpringBoneJobs.AnglelimitTypes.Hinge:
                            {
                                Handles.color = Color.cyan;
                                Handles.DrawWireArc(Vector3.zero, Vector3.left,
                                    new Vector3(0, c, s) * limit_tail_pos.magnitude,
                                    m_target.m_angleLimitAngle1 * Mathf.Rad2Deg,
                                    limit_tail_pos.magnitude
                                );
                                // yz plane
                                //     o   o head
                                //    / \
                                //   /   \
                                // -r -+- r
                                //
                                Handles.DrawLine(Vector3.zero, new Vector3(0, c, s) * limit_tail_pos.magnitude);
                                Handles.DrawLine(Vector3.zero, new Vector3(0, c, -s) * limit_tail_pos.magnitude);
                                break;
                            }

                        case UniGLTF.SpringBoneJobs.AnglelimitTypes.Spherical:
                            {
                                Handles.color = Color.cyan;
                                Handles.DrawWireArc(Vector3.zero, Vector3.left,
                                    new Vector3(0, c, s) * limit_tail_pos.magnitude,
                                    m_target.m_angleLimitAngle1 * Mathf.Rad2Deg,
                                    limit_tail_pos.magnitude
                                );
                                // yz plane
                                //     o   o head
                                //    / \
                                //   /   \
                                // -r -+- r
                                //
                                Handles.DrawLine(Vector3.zero, new Vector3(0, c, s) * limit_tail_pos.magnitude);
                                Handles.DrawLine(Vector3.zero, new Vector3(0, c, -s) * limit_tail_pos.magnitude);
                                break;
                            }
                    }
                }
            }
        }
    }
}