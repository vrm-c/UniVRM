using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniGLTF.M17N;
using UniGLTF.Utils;
using UnityEngine;

namespace UniGLTF
{
    public enum EnableTPose
    {
        [LangMsg(Languages.ja, "このボタンで自動で T-Pose にできます。手動で T-Pose にしたり、ボタンの後で手直ししてもOKです。")]
        [LangMsg(Languages.en, "T-Pose can be made automatically with this button, or you can make the model as T-Pose manually. Adjusting T-Pose manually after applying this function is also OK")]
        ENALBE_TPOSE_BUTTON,

        [LangMsg(Languages.ja, "このボタンで自動で T-Pose にできます。prefab には実行できません。")]
        [LangMsg(Languages.en, "T-Pose can be made automatically with this button. It cannot be run on prefabs.")]
        DISABLE_TPOSE_BUTTON,
    }

    public static class InternalTPose
    {
        struct TRS
        {
            public Vector3 T;
            public Quaternion R;
            public Vector3 S;

            public void Apply(Transform t)
            {
                t.localPosition = T;
                t.localRotation = R;
                t.localScale = S;
            }

            public static TRS From(Transform t)
            {
                return new TRS
                {
                    T = t.localPosition,
                    R = t.localRotation,
                    S = t.localScale,
                };
            }
        }

        static void BuildMap(Transform t, Dictionary<string, TRS> map, Transform root = null)
        {
            if (root == null)
            {
                root = t;
            }

            map.Add(t.RelativePathFrom(root), TRS.From(t));

            foreach (Transform child in t)
            {
                BuildMap(child, map, root);
            }
        }

        static void ApplyMap(Transform t, Dictionary<string, TRS> map, Transform root = null)
        {
            if (root == null)
            {
                root = t;
            }

            map[t.RelativePathFrom(root)].Apply(t);

            foreach (Transform child in t)
            {
                ApplyMap(child, map, root);
            }
        }

        public static bool TrySampleBindPose(GameObject go)
        {
            // https://forum.unity.com/threads/mesh-bindposes.383752/
            var type = Type.GetType("UnityEditor.AvatarSetupTool, UnityEditor");
            if (type != null)
            {
                var info = type.GetMethod("SampleBindPose", BindingFlags.Static | BindingFlags.Public);
                if (info != null)
                {
                    // prefab cause error message. create copy
                    var clone = GameObject.Instantiate(go);
                    try
                    {
                        info.Invoke(null, new object[] { clone });
                        var map = new Dictionary<string, TRS>();
                        BuildMap(clone.transform, map);
                        ApplyMap(go.transform, map);
                    }
                    finally
                    {
                        GameObject.DestroyImmediate(clone);
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool TryMakePoseValid(GameObject go)
        {
            var type = Type.GetType("UnityEditor.AvatarSetupTool, UnityEditor");
            if (type == null)
            {
                return false;
            }

            // public static Dictionary<Transform, bool> GetModelBones(Transform root, bool includeAll, BoneWrapper[] humanBones)
            var GetModelBones = type.GetMethod("GetModelBones", BindingFlags.Static | BindingFlags.Public);
            if (GetModelBones == null)
            {
                return false;
            }

            // BoneWrapper[] humanBones = GetHumanBones(existingMappings, modelBones);
            var GetHumanBones = type.GetMethods(BindingFlags.Static | BindingFlags.Public).First(x =>
            {
                if (x.Name != "GetHumanBones")
                {
                    return false;
                }
                if (x.GetParameters()[0].Name != "existingMappings")
                {
                    return false;
                }

                return true;
            });
            if (GetHumanBones == null)
            {
                return false;
            }

            // public static void MakePoseValid(BoneWrapper[] bones)
            var MakePoseValid = type.GetMethod("MakePoseValid", BindingFlags.Static | BindingFlags.Public);
            if (MakePoseValid == null)
            {
                return false;
            }

            var modelBones = GetModelBones.Invoke(null, new object[] { go.transform, false, null });
            var existingMappings = new Dictionary<string, string>();

            var animator = go.GetComponent<Animator>();
            foreach (var bone in CachedEnum.GetValues<HumanBodyBones>())
            {
                if (bone == HumanBodyBones.LastBone)
                {
                    continue;
                }
                var t = animator.GetBoneTransform(bone);
                if (t != null)
                {
                    existingMappings.Add(bone.ToString(), t.name);
                }
            }

            var humanBones = GetHumanBones.Invoke(null, new object[] { existingMappings, modelBones });
            MakePoseValid.Invoke(null, new object[] { humanBones });

            return true;
        }
    }
}
