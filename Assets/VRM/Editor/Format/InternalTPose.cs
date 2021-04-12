using System;
using System.Collections.Generic;
using System.Reflection;
using UniGLTF;
using UnityEngine;

namespace VRM
{
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
    }
}