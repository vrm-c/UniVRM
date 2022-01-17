using System;
using System.Linq;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;


namespace VRM
{
    public static class glTF_VRMExtensions
    {
        [Obsolete("Use Create(root, meshes, binding)")]
        public static glTF_VRM_BlendShapeBind Cerate(Transform root, BlendShapeBinding binding,
            gltfExporter exporter)
        {
            return Create(root, binding, exporter);
        }

        public static glTF_VRM_BlendShapeBind Create(Transform root, BlendShapeBinding binding,
            gltfExporter exporter)
        {
            if (root == null || exporter == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty((binding.RelativePath)))
            {
                Debug.LogWarning("binding.RelativePath is null");
                return null;
            }
            var found = root.transform.Find(binding.RelativePath);
            if (found == null)
            {
                var name = binding.RelativePath.Split('/').Last();
                found = root.GetComponentsInChildren<Transform>().FirstOrDefault(x => x.name == name);
                if (found == null)
                {
                    Debug.LogWarning($"{binding.RelativePath} not found");
                    return null;
                }
                else
                {
                    Debug.LogWarning($"fall back '{binding.RelativePath}' => '{found.RelativePathFrom(root)}'");
                }
            }
            var renderer = found.GetComponent<SkinnedMeshRenderer>();
            if (renderer == null)
            {
                return null;
            }

            if (!renderer.gameObject.activeInHierarchy)
            {
                return null;
            }

            var mesh = renderer.sharedMesh;
            var meshIndex = exporter.Meshes.IndexOf(mesh);
            if (meshIndex == -1)
            {
                return null;
            }

            if (!exporter.MeshBlendShapeIndexMap.TryGetValue(mesh, out Dictionary<int, int> blendShapeIndexMap))
            {
                // この Mesh は  エクスポートされていない
                return null;
            }

            if (!blendShapeIndexMap.TryGetValue(binding.Index, out int blendShapeIndex))
            {
                // この blendShape は エクスポートされていない(空だった？)
                return null;
            }

            return new glTF_VRM_BlendShapeBind
            {
                mesh = meshIndex,
                index = blendShapeIndex,
                weight = binding.Weight,
            };
        }

        public static void Add(this glTF_VRM_BlendShapeMaster master,
            BlendShapeClip clip, gltfExporter exporter)
        {
            master.blendShapeGroups.Add(clip.Serialize(exporter));
        }

        public static glTF_VRM_BlendShapeGroup Serialize(this BlendShapeClip clip, gltfExporter exporter)
        {
            var bindList = new List<glTF_VRM_BlendShapeBind>();
            if (clip.Values != null && exporter != null)
            {
                foreach (var value in clip.Values)
                {
                    var bind = Create(exporter.Copy.transform, value, exporter);
                    if (bind == null)
                    {
                        // Debug.LogFormat("{0}: skip blendshapebind", clip.name);
                        continue;
                    }
                    bindList.Add(bind);
                }
            }

            var materialValueBinds = new List<glTF_VRM_MaterialValueBind>();
            if (clip.MaterialValues != null)
            {
                materialValueBinds.AddRange(clip.MaterialValues.Select(y => new glTF_VRM_MaterialValueBind
                {
                    materialName = y.MaterialName,
                    propertyName = y.ValueName,
                    targetValue = y.TargetValue.ToArray(),
                }));
            }

            return new glTF_VRM_BlendShapeGroup
            {
                name = clip.BlendShapeName,
                presetName = clip.Preset.ToString().ToLowerInvariant(),
                isBinary = clip.IsBinary,
                binds = bindList,
                materialValues = materialValueBinds,
            };
        }

        public static void Apply(this glTF_VRM_DegreeMap map, CurveMapper mapper)
        {
            map.curve = mapper.Curve.keys.SelectMany(x => new float[] { x.time, x.value, x.inTangent, x.outTangent }).ToArray();
            map.xRange = mapper.CurveXRangeDegree;
            map.yRange = mapper.CurveYRangeDegree;
        }
    }
}
