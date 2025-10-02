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
        private SerializedProperty m_angleLimitPitch;
        private SerializedProperty m_angleLimitYaw;

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
            m_angleLimitRotation = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_limitSpaceOffset));
            m_angleLimitPitch = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_pitch));
            m_angleLimitYaw = serializedObject.FindProperty(nameof(VRM10SpringBoneJoint.m_yaw));

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
            var fold = EditorGUILayout.Foldout(m_showAnglelimitSettings, "AngleLimit Settings (experimental)");
            if (m_showAnglelimitSettings != fold)
            {
                m_showAnglelimitSettings = fold;
                SceneView.RepaintAll();
            }
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
                        EditorGUILayout.PropertyField(m_angleLimitPitch);
                        break;

                    case UniGLTF.SpringBoneJobs.AnglelimitTypes.Hinge:
                        EditorGUILayout.PropertyField(m_angleLimitRotation);
                        EditorGUILayout.PropertyField(m_angleLimitPitch);
                        break;

                    case UniGLTF.SpringBoneJobs.AnglelimitTypes.Spherical:
                        EditorGUILayout.PropertyField(m_angleLimitRotation);
                        EditorGUILayout.PropertyField(m_angleLimitPitch);
                        EditorGUILayout.PropertyField(m_angleLimitYaw);
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
            Quaternion rot = Handles.RotationHandle(m_target.m_limitSpaceOffset, Vector3.zero);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "m_angleLimitRotation");
                m_target.m_limitSpaceOffset = rot;
                return true;
            }
            else
            {
                return false;
            }
        }

        static void DrawChain(Vrm10InstanceSpringBone.Spring spring)
        {
            Handles.color = new Color(1, 0.5f, 0);
            var head = spring.Joints[0];
            for (int i = 1; i < spring.Joints.Count; ++i)
            {
                var tail = spring.Joints[i];
                if (head != null && tail != null)
                {
                    Handles.DrawLine(head.transform.position, tail.transform.position);
                }
                head = tail;
            }
        }

        static void DrawSpace(Matrix4x4 limitSpace, float size)
        {
            float half = size * 0.5f;
            Handles.matrix = limitSpace;
            Handles.color = Color.red;
            var x = Vector3.right * half;
            Handles.DrawLine(x, -x);
            Handles.color = Color.green;
            Handles.DrawLine(Vector3.zero, Vector3.up * size);
            Handles.color = Color.blue;
            var y = Vector3.forward * half;
            Handles.DrawLine(-y, y);

            Handles.color = new Color(1, 1, 1, 0.1f);
            Handles.DrawSolidDisc(Vector3.zero, Vector3.up, half);
        }

        void OnSceneGUI()
        {
            Tools.hidden = false;

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
            DrawChain(spring);

            if (j + 1 < spring.Joints.Count)
            {
                var _tail = spring.Joints[j + 1];
                if (_tail != null)
                {
                    var tail = _tail.transform;
                    var local_axis = head.worldToLocalMatrix.MultiplyPoint(tail.position);
                    var limit_tail_pos = Vector3.up * local_axis.magnitude;
                    var limitRotation = calcSpringboneLimitSpace(head.rotation, local_axis);

                    if (m_showAnglelimitSettings)
                    {
                        Tools.hidden = true;
                        if (m_target.m_anglelimitType != UniGLTF.SpringBoneJobs.AnglelimitTypes.None)
                        {
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
                    }

                    limitRotation = limitRotation * m_target.m_limitSpaceOffset;

                    var limitSpace = Matrix4x4.TRS(head.position, limitRotation, Vector3.one);
                    DrawSpace(limitSpace, limit_tail_pos.magnitude);

                    switch (m_target.m_anglelimitType)
                    {
                        case UniGLTF.SpringBoneJobs.AnglelimitTypes.Cone:
                            DrawCone(limit_tail_pos, m_target.m_pitch);
                            break;

                        case UniGLTF.SpringBoneJobs.AnglelimitTypes.Hinge:
                            DrawHinge(limit_tail_pos, m_target.m_pitch, Color.cyan);
                            break;

                        case UniGLTF.SpringBoneJobs.AnglelimitTypes.Spherical:
                            DrawHinge(limit_tail_pos, m_target.m_pitch, Color.cyan * 0.5f);
                            DrawSpherical(limit_tail_pos, m_target.m_pitch, m_target.m_yaw);
                            break;
                    }
                }
            }
        }

        private static void DrawCone(in Vector3 limit_tail_pos, float pitch)
        {
            var s = Mathf.Sin(pitch);
            var c = Mathf.Cos(pitch);

            Handles.color = Color.cyan;
            var r = Mathf.Tan(pitch) * limit_tail_pos.magnitude * c;
            Handles.DrawWireDisc(limit_tail_pos * c, Vector3.up, r, 1);
            //         o head
            //      r /
            //      |/
            // -r --+-- r
            //      |
            //      -r
            var pz = new Vector3(0, c, s) * limit_tail_pos.magnitude;
            var nz = new Vector3(0, c, -s) * limit_tail_pos.magnitude;
            var px = new Vector3(s, c, 0) * limit_tail_pos.magnitude;
            var nx = new Vector3(-s, c, 0) * limit_tail_pos.magnitude;
            Handles.DrawLine(Vector3.zero, pz);
            Handles.DrawLine(Vector3.zero, nz);
            Handles.DrawLine(Vector3.zero, px);
            Handles.DrawLine(Vector3.zero, nx);

            Handles.color = new Color(0, 1, 1, 0.1f);
            Handles.Label(Vector3.Slerp(limit_tail_pos, pz, 0.5f) * 0.5f, $"pitch: {pitch * Mathf.Rad2Deg:F0}°");
            Handles.DrawSolidArc(Vector3.zero, Vector3.Cross(limit_tail_pos, pz),
                limit_tail_pos,
                pitch * Mathf.Rad2Deg,
                limit_tail_pos.magnitude * 0.5f
            );
        }

        private static void DrawHinge(in Vector3 limit_tail_pos, float pitch, Color color)
        {
            var s = Mathf.Sin(pitch);
            var c = Mathf.Cos(pitch);

            // yz plane
            //     o   o head
            //    / \
            //   /   \
            // -r -+- r
            //
            var a = new Vector3(0, c, s) * limit_tail_pos.magnitude;
            var b = new Vector3(0, c, -s) * limit_tail_pos.magnitude;
            Handles.color = color;
            Handles.DrawLine(Vector3.zero, a);
            Handles.DrawLine(Vector3.zero, b);

            Handles.DrawWireArc(Vector3.zero, Vector3.left,
                new Vector3(0, c, s),
                pitch * 2 * Mathf.Rad2Deg,
                limit_tail_pos.magnitude
            );

            color.a = 0.1f;
            Handles.color = color;
            Handles.Label(Vector3.Slerp(limit_tail_pos, a, 0.5f) * 0.5f, $"pitch: {pitch * Mathf.Rad2Deg:F0}°");
            Handles.DrawSolidArc(Vector3.zero, Vector3.left,
                new Vector3(0, c, s),
                pitch * Mathf.Rad2Deg,
                limit_tail_pos.magnitude * 0.5f
            );
        }

        private static void DrawSpherical(in Vector3 limit_tail_pos, float pitch, float yaw)
        {
            Handles.color = Color.cyan;

            var ts = Mathf.Sin(pitch);
            var tc = Mathf.Cos(pitch);
            var ps = Mathf.Sin(yaw);
            var pc = Mathf.Cos(yaw);

            // y     = tc * pc
            // ^ z   = tc * ps
            // |/
            // +-> x = ts
            var x = ps;
            var y = pc * tc;
            var z = pc * ts;

            //  z
            //  ^
            // b|a 
            // -+->x
            // c|d
            var a = new Vector3(x, y, z);
            var b = new Vector3(-x, y, z);
            var c = new Vector3(-x, y, -z);
            var d = new Vector3(x, y, -z);

            Handles.DrawLine(Vector3.zero, a * limit_tail_pos.magnitude);
            Handles.DrawLine(Vector3.zero, b * limit_tail_pos.magnitude);
            Handles.DrawLine(Vector3.zero, c * limit_tail_pos.magnitude);
            Handles.DrawLine(Vector3.zero, d * limit_tail_pos.magnitude);

            // ab / cd
            Handles.DrawWireArc(Vector3.zero, Vector3.Cross(a, b).normalized,
                a * limit_tail_pos.magnitude,
                Vector3.Angle(a, b),
                limit_tail_pos.magnitude
            );
            Handles.DrawWireArc(Vector3.zero, Vector3.Cross(c, d).normalized,
                c * limit_tail_pos.magnitude,
                Vector3.Angle(c, d),
                limit_tail_pos.magnitude
            );
            Handles.Label(Vector3.Slerp(a, b, 0.25f) * limit_tail_pos.magnitude, $"yaw: {yaw * Mathf.Rad2Deg:F0}°");

            // bc / da
            Handles.DrawWireArc(Vector3.zero, Vector3.Cross(b, c).normalized,
                b * limit_tail_pos.magnitude,
                Vector3.Angle(b, c),
                limit_tail_pos.magnitude
            );
            Handles.DrawWireArc(Vector3.zero, Vector3.Cross(d, a).normalized,
                d * limit_tail_pos.magnitude,
                Vector3.Angle(d, a),
                limit_tail_pos.magnitude
            );
        }
    }
}