using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace UniHumanoid
{
    [CustomEditor(typeof(BoneMapping))]
    public class BoneMappingEditor : Editor
    {
        BoneMapping m_target;

        void OnEnable()
        {
            m_target = (BoneMapping)target;

            var animator = m_target.GetComponent<Animator>();
            if (animator != null)
            {
                m_bones = EachBoneDefs.Select(x => new Bone(
animator.GetBoneTransform(x.Head), animator.GetBoneTransform(x.Tail)))
.Where(x => x.Head != null && x.Tail != null)
.ToArray();
            }
        }

        static GameObject ObjectField(GameObject obj)
        {
            return (GameObject)EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
        }

        static GameObject ObjectField(string label, GameObject obj)
        {
            return (GameObject)EditorGUILayout.ObjectField(label, obj, typeof(GameObject), true);
        }

        const int LABEL_WIDTH = 100;

        static void BoneField(HumanBodyBones bone, GameObject[] bones)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(bone.ToString(), GUILayout.Width(LABEL_WIDTH));
            bones[(int)bone] = ObjectField(bones[(int)bone]);
            EditorGUILayout.EndHorizontal();
        }

        static void BoneField(HumanBodyBones left, HumanBodyBones right, GameObject[] bones)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(left.ToString().Substring(4), GUILayout.Width(LABEL_WIDTH)); // skip left
            bones[(int)left] = ObjectField(bones[(int)left]);
            bones[(int)right] = ObjectField(bones[(int)right]);
            EditorGUILayout.EndHorizontal();
        }

        bool m_handFoldout;
        bool m_settingsFoldout;

        public override void OnInspectorGUI()
        {
            var bones = m_target.Bones;
            if (bones == null)
            {
                return;
            }

            BoneField(HumanBodyBones.Hips, bones);

            if (bones[(int)HumanBodyBones.Hips] == null)
            {
                EditorGUILayout.HelpBox(@"First, you set hips", MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Guess bone mapping"))
                {
                    m_target.GuessBoneMapping();
                }
                EditorGUILayout.HelpBox(@"Guess bones from hips", MessageType.Info);

                if (GUILayout.Button("Ensure T-Pose"))
                {
                    m_target.EnsureTPose();
                }
                EditorGUILayout.HelpBox(@"Arms to Horizontal", MessageType.Info);

                if (GUILayout.Button("Create avatar"))
                {
                    var description = AvatarDescription.Create(m_target.Description);
                    BoneMapping.SetBonesToDescription(m_target, description);
                    var avatar = description.CreateAvatarAndSetup(m_target.transform);
                    if (avatar != null)
                    {
                        avatar.name = "avatar";
#if UNITY_2018_2_OR_NEWER
                        var prefabRoot = PrefabUtility.GetCorrespondingObjectFromSource(m_target.gameObject);
#else
                        var prefabRoot = PrefabUtility.GetPrefabParent(m_target.gameObject);
#endif
                        var prefabPath = AssetDatabase.GetAssetPath(prefabRoot);

                        var path = (string.IsNullOrEmpty(prefabPath))
                            ? string.Format("Assets/{0}.asset", avatar.name)
                            : string.Format("{0}/{1}.asset", Path.GetDirectoryName(prefabPath), Path.GetFileNameWithoutExtension(prefabPath))
                            ;
                        path = EditorUtility.SaveFilePanel(
                                "Save avatar",
                                Path.GetDirectoryName(path),
                                string.Format("{0}.avatar.asset", serializedObject.targetObject.name),
                                "asset");
                        var assetPath = HumanPoseTransferEditor.ToAssetPath(path);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            AssetDatabase.CreateAsset(description, assetPath); // overwrite
                            AssetDatabase.AddObjectToAsset(avatar, assetPath);

                            Debug.LogFormat("Create avatar {0}", path);
                            AssetDatabase.ImportAsset(assetPath);
                            Selection.activeObject = avatar;
                        }
                        else
                        {
                            Debug.LogWarning("fail to CreateAvatar");
                        }
                    }
                }
                EditorGUILayout.HelpBox(@"before create,

1. Model root transform should reset(origin without rotation)
2. Model forward to Z+(rotate child of model root)
3. Required bones filled
", MessageType.Info);
            }

            /*
            m_settingsFoldout = EditorGUILayout.Foldout(m_settingsFoldout, "AvatarSettings");
            if (m_settingsFoldout)
            {
                EditorGUILayout.FloatField("armStretch", m_target.armStretch);
                EditorGUILayout.FloatField("legStretch", m_target.legStretch);
                EditorGUILayout.FloatField("upperArmTwist", m_target.upperArmTwist);
                EditorGUILayout.FloatField("lowerArmTwist", m_target.lowerArmTwist);
                EditorGUILayout.FloatField("upperLegTwist", m_target.upperLegTwist);
                EditorGUILayout.FloatField("lowerLegTwist", m_target.lowerLegTwist);
                EditorGUILayout.FloatField("feetSpacing", m_target.feetSpacing);
                EditorGUILayout.Toggle("hasTranslationDoF", m_target.hasTranslationDoF);
                //public BoneLimit[] human;
            }
            */

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Arm", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
            EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(40));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
            BoneField(HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder, bones);
            BoneField(HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm, bones);
            BoneField(HumanBodyBones.LeftLowerArm, HumanBodyBones.RightLowerArm, bones);
            BoneField(HumanBodyBones.LeftHand, HumanBodyBones.RightHand, bones);

            EditorGUILayout.LabelField("Body and Head", EditorStyles.boldLabel);
            BoneField(HumanBodyBones.Spine, bones);
            BoneField(HumanBodyBones.Chest, bones);
#if UNITY_5_6_OR_NEWER
            BoneField(HumanBodyBones.UpperChest, bones);
#endif
            BoneField(HumanBodyBones.Neck, bones);
            BoneField(HumanBodyBones.Head, bones);
            BoneField(HumanBodyBones.Jaw, bones);
            BoneField(HumanBodyBones.LeftEye, HumanBodyBones.RightEye, bones);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Leg", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
            EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(40));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
            BoneField(HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg, bones);
            BoneField(HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg, bones);
            BoneField(HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, bones);
            BoneField(HumanBodyBones.LeftToes, HumanBodyBones.RightToes, bones);

            m_handFoldout = EditorGUILayout.Foldout(m_handFoldout, "Hand");
            if (m_handFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Thumb", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
                EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
                BoneField(HumanBodyBones.LeftThumbProximal, HumanBodyBones.RightThumbProximal, bones);
                BoneField(HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.RightThumbIntermediate, bones);
                BoneField(HumanBodyBones.LeftThumbDistal, HumanBodyBones.RightThumbDistal, bones);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Index", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
                EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
                BoneField(HumanBodyBones.LeftIndexProximal, HumanBodyBones.RightIndexProximal, bones);
                BoneField(HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.RightIndexIntermediate, bones);
                BoneField(HumanBodyBones.LeftIndexDistal, HumanBodyBones.RightIndexDistal, bones);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Middle", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
                EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
                BoneField(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.RightMiddleProximal, bones);
                BoneField(HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.RightMiddleIntermediate, bones);
                BoneField(HumanBodyBones.LeftMiddleDistal, HumanBodyBones.RightMiddleDistal, bones);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ring", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
                EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
                BoneField(HumanBodyBones.LeftRingProximal, HumanBodyBones.RightRingProximal, bones);
                BoneField(HumanBodyBones.LeftRingIntermediate, HumanBodyBones.RightRingIntermediate, bones);
                BoneField(HumanBodyBones.LeftRingDistal, HumanBodyBones.RightRingDistal, bones);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Little", EditorStyles.boldLabel, GUILayout.Width(LABEL_WIDTH));
                EditorGUILayout.LabelField("Left", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Right", EditorStyles.boldLabel, GUILayout.Width(40));
                EditorGUILayout.EndHorizontal();
                BoneField(HumanBodyBones.LeftLittleProximal, HumanBodyBones.RightLittleProximal, bones);
                BoneField(HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.RightLittleIntermediate, bones);
                BoneField(HumanBodyBones.LeftLittleDistal, HumanBodyBones.RightLittleDistal, bones);
            }

            EditorUtility.SetDirty(m_target);
        }

        struct Bone
        {
            public Transform Head;
            public Transform Tail;

            public Bone(Transform head, Transform tail)
            {
                Head = head;
                Tail = tail;
            }

            public void Draw()
            {
                Handles.DrawLine(Head.transform.position, Tail.transform.position);
            }
        }

        Bone[] m_bones;

        struct BoneDef
        {
            public HumanBodyBones Head;
            public HumanBodyBones Tail;

            public BoneDef(HumanBodyBones head, HumanBodyBones tail)
            {
                Head = head;
                Tail = tail;
            }
        }
        static readonly HumanBodyBones[][] BoneDefs =
        {
            new HumanBodyBones[]
            {
                HumanBodyBones.Hips,
                HumanBodyBones.Spine,
                HumanBodyBones.Chest,
                HumanBodyBones.Neck,
                HumanBodyBones.Head,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.Chest,
                HumanBodyBones.LeftShoulder,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.LeftHand,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.Chest,
                HumanBodyBones.RightShoulder,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.RightHand,
            },

            new HumanBodyBones[]
            {
                HumanBodyBones.LeftThumbProximal,
                HumanBodyBones.LeftThumbIntermediate,
                HumanBodyBones.LeftThumbDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.LeftIndexProximal,
                HumanBodyBones.LeftIndexIntermediate,
                HumanBodyBones.LeftIndexDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.LeftMiddleProximal,
                HumanBodyBones.LeftMiddleIntermediate,
                HumanBodyBones.LeftMiddleDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.LeftRingProximal,
                HumanBodyBones.LeftRingIntermediate,
                HumanBodyBones.LeftRingDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.LeftLittleProximal,
                HumanBodyBones.LeftLittleIntermediate,
                HumanBodyBones.LeftLittleDistal,
            },

            new HumanBodyBones[]
            {
                HumanBodyBones.RightThumbProximal,
                HumanBodyBones.RightThumbIntermediate,
                HumanBodyBones.RightThumbDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.RightIndexProximal,
                HumanBodyBones.RightIndexIntermediate,
                HumanBodyBones.RightIndexDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.RightMiddleProximal,
                HumanBodyBones.RightMiddleIntermediate,
                HumanBodyBones.RightMiddleDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.RightRingProximal,
                HumanBodyBones.RightRingIntermediate,
                HumanBodyBones.RightRingDistal,
            },
            new HumanBodyBones[]
            {
                HumanBodyBones.RightLittleProximal,
                HumanBodyBones.RightLittleIntermediate,
                HumanBodyBones.RightLittleDistal,
            },

            new HumanBodyBones[]
            {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.LeftFoot,
            },

            new HumanBodyBones[]
            {
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.RightFoot,
            },
        };
        static IEnumerable<BoneDef> EachBoneDefs
        {
            get
            {
                foreach (var x in BoneDefs)
                {
                    var count = x.Length - 1;
                    for (int i = 0; i < count; ++i)
                    {
                        yield return new BoneDef(x[i], x[i + 1]);
                    }
                }
            }
        }

        void DrawBone(HumanBodyBones bone, GameObject go)
        {
            if (go == null)
            {
                return;
            }

            Handles.Label(go.transform.position,
                go.name + "\n(" + bone.ToString() + ")");
        }

        private void OnSceneGUI()
        {
            var bones = m_target.Bones;
            if (bones != null)
            {
                for (int i = 0; i < bones.Length; ++i)
                {
                    DrawBone((HumanBodyBones)i, bones[i]);
                }
                foreach(var x in m_bones)
                {
                    x.Draw();
                }
            }
        }
    }
}
