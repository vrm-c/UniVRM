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
            if (string.IsNullOrEmpty((binding.RelativePath)))
            {
                Debug.LogWarning("binding.RelativePath is null");
                return null;
            }
            var found = root.transform.Find(binding.RelativePath);
            if (found == null)
            {
                var name = binding.RelativePath.Split('/').Last();
                found = root.GetComponentsInChildren<Transform>().Where(x => x.name == name).First();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="master"></param>
        /// <param name="clip"></param>
        /// <param name="transform"></param>
        /// <param name="meshes"></param>
        /// <param name="blendShapeIndexMap">エクスポート中にBlendShapeIndexが変わったかもしれない</param>
        public static void Add(this glTF_VRM_BlendShapeMaster master,
            BlendShapeClip clip, gltfExporter exporter)
        {
            var list = new List<glTF_VRM_BlendShapeBind>();
            if (clip.Values != null)
            {
                foreach (var value in clip.Values)
                {
                    var bind = Create(exporter.Copy.transform, value, exporter);
                    if (bind == null)
                    {
                        // Debug.LogFormat("{0}: skip blendshapebind", clip.name);
                        continue;
                    }
                    list.Add(bind);
                }
            }

            var materialList = new List<glTF_VRM_MaterialValueBind>();
            if (clip.MaterialValues != null)
            {
                materialList.AddRange(clip.MaterialValues.Select(y => new glTF_VRM_MaterialValueBind
                {
                    materialName = y.MaterialName,
                    propertyName = y.ValueName,
                    targetValue = y.TargetValue.ToArray(),
                }));
            }

            var group = new glTF_VRM_BlendShapeGroup
            {
                name = clip.BlendShapeName,
                presetName = clip.Preset.ToString().ToLower(),
                isBinary = clip.IsBinary,
                binds = list,
                materialValues = materialList,
            };
            master.blendShapeGroups.Add(group);
        }

        public static void Apply(this glTF_VRM_DegreeMap map, CurveMapper mapper)
        {
            map.curve = mapper.Curve.keys.SelectMany(x => new float[] { x.time, x.value, x.inTangent, x.outTangent }).ToArray();
            map.xRange = mapper.CurveXRangeDegree;
            map.yRange = mapper.CurveYRangeDegree;
        }
    }
}
