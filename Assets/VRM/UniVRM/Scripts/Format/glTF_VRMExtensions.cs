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
        public static glTF_VRM_BlendShapeBind Cerate(Transform root, List<Mesh> meshes, BlendShapeBinding binding)
        {
            return Create(root, meshes, binding);
        }

        public static glTF_VRM_BlendShapeBind Create(Transform root, List<Mesh> meshes, BlendShapeBinding binding)
        {
            var transform = UniGLTF.UnityExtensions.GetFromPath(root.transform, binding.RelativePath);
            var renderer = transform.GetComponent<SkinnedMeshRenderer>();
            var mesh = renderer.sharedMesh;
            var meshIndex = meshes.IndexOf(mesh);

            return new glTF_VRM_BlendShapeBind
            {
                mesh = meshIndex,
                index = binding.Index,
                weight = binding.Weight,
            };
        }

        public static void Add(this glTF_VRM_BlendShapeMaster master,
            BlendShapeClip clip, Transform transform, List<Mesh> meshes)
        {
            var list = new List<glTF_VRM_BlendShapeBind>();
            if (clip.Values != null)
            {
                list.AddRange(clip.Values.Select(y => Create(transform, meshes.ToList(), y)));
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
