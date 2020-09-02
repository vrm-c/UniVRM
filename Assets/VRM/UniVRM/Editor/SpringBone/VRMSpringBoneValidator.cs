using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VRM
{
    static class VRMSpringBoneValidator
    {
        // https://github.com/vrm-c/UniVRM/issues/474
        public static IEnumerable<Validation> Validate(GameObject root)
        {
            if (root == null)
            {
                yield break;
            }

            var hierarchy = root.GetComponentsInChildren<Transform>(true);

            Dictionary<Transform, List<VRMSpringBone>> rootMap = new Dictionary<Transform, List<VRMSpringBone>>();

            foreach (var sb in root.GetComponentsInChildren<VRMSpringBone>())
            {
                for (int i = 0; i < sb.RootBones.Count; ++i)
                {
                    var springRoot = sb.RootBones[i];
                    if (springRoot == null)
                    {
                        yield return Validation.Error($"[VRMSpringBone]{sb.name}.RootBones[{i}] is null");
                        continue;
                    }
                    if (!hierarchy.Contains(springRoot))
                    {
                        yield return Validation.Error($"[VRMSpringBone]{sb.name}.RootBones[{i}] is out of hierarchy");
                        continue;
                    }
                    if (!springRoot.transform.EnableForExport())
                    {
                        yield return Validation.Error($"[VRMSpringBone]{sb.name}.RootBones[{i}] is not active");
                        continue;
                    }

                    if (!rootMap.TryGetValue(springRoot, out List<VRMSpringBone> list))
                    {
                        list = new List<VRMSpringBone>();
                        rootMap.Add(springRoot, list);
                    }
                    list.Add(sb);
                }

                for (int i = 0; i < sb.ColliderGroups.Length; ++i)
                {
                    var c = sb.ColliderGroups[i];
                    if (c == null)
                    {
                        yield return Validation.Error($"{sb.name}.ColliderGroups[{i}] is null");
                        continue;
                    }
                    if (!hierarchy.Contains(c.transform))
                    {
                        yield return Validation.Error($"{sb.name}.ColliderGroups[{i}] is out of hierarchy");
                        continue;
                    }
                }
            }

            foreach (var kv in rootMap)
            {
                if (kv.Value.Count > 1)
                {
                    // * GameObjectが複数回ルートとして指定されてる(SpringBoneが同じでも別でも)
                    var list = string.Join(", ", kv.Value.Select(x => string.IsNullOrEmpty(x.m_comment) ? x.name : x.m_comment));
                    yield return Validation.Warning($"{kv.Key} found multiple. {list}");
                }

                var rootInRootMap = new Dictionary<Transform, List<Transform>>();
                foreach (var child in kv.Key.GetComponentsInChildren<Transform>())
                {
                    // * Rootから子をだどって、別のRootが現れる
                    if (child == kv.Key)
                    {
                        continue;
                    }

                    if (!rootMap.ContainsKey(child))
                    {
                        continue;
                    }

                    if (!rootInRootMap.TryGetValue(kv.Key, out List<Transform> rootInRoot))
                    {
                        rootInRoot = new List<Transform>();
                        rootInRootMap.Add(kv.Key, rootInRoot);
                    }
                    rootInRoot.Add(child);
                }
                foreach (var rootList in rootInRootMap)
                {
                    var list = string.Join(", ", rootList.Value.Select(x => x.name));
                    yield return Validation.Warning($"{rootList.Key} hierarchy contains other root: {list}");
                }
            }
        }
    }
}
