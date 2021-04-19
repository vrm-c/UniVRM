using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace VRM
{
    public static class VRMBlendShapeProxyValidator
    {
        public static IEnumerable<Validation> Validate(this VRMBlendShapeProxy p, GameObject _)
        {
            if (p == null)
            {
                yield return Validation.Error("VRMBlendShapeProxy is null");
                yield break;
            }

            if (p.BlendShapeAvatar == null)
            {
                yield return Validation.Error("BlendShapeAvatar is null");
                yield break;
            }

            // presetがユニークか
            var used = new HashSet<BlendShapeKey>();
            foreach (var c in p.BlendShapeAvatar.Clips)
            {
                var key = c.Key;
                if (used.Contains(key))
                {
                    yield return Validation.Error($"duplicated BlendShapeKey: {key}");
                }
                else
                {
                    used.Add(key);
                }
            }

            var materialNames = new HashSet<string>();
            foreach (var r in p.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var m in r.sharedMaterials)
                {
                    if (m != null)
                    {
                        if (!materialNames.Contains(m.name))
                        {
                            materialNames.Add(m.name);
                        }
                    }
                }
            }

            // 参照が生きているか
            foreach (var c in p.BlendShapeAvatar.Clips)
            {
                for (int i = 0; i < c.Values.Length; ++i)
                {
                    var v = c.Values[i];
                    var target = p.transform.Find(v.RelativePath);
                    if (target == null)
                    {
                        yield return Validation.Warning($"{c}.Values[{i}].RelativePath({v.RelativePath} is not found");
                    }
                }

                for (int i = 0; i < c.MaterialValues.Length; ++i)
                {
                    var v = c.MaterialValues[i];
                    if (!materialNames.Contains(v.MaterialName))
                    {
                        yield return Validation.Warning($"{c}.MaterialValues[{i}].MaterialName({v.MaterialName} is not found");
                    }
                }
            }
        }
    }
}
