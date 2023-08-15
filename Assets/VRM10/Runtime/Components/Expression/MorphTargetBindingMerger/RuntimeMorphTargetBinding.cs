using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVRM10
{
    internal readonly struct RuntimeMorphTargetBinding : IEquatable<RuntimeMorphTargetBinding>
    {
        public static RuntimeMorphTargetBinding? Create(MorphTargetBinding binding, Transform modelRoot)
        {
            if (modelRoot == null) return null;

            var targetGameObject = modelRoot.Find(binding.RelativePath);
            if (targetGameObject == null) return null;

            var targetRenderer = targetGameObject.GetComponent<SkinnedMeshRenderer>();
            if (targetRenderer == null) return null;
            if (targetRenderer.sharedMesh == null) return null;
            if (targetRenderer.sharedMesh.blendShapeCount <= binding.Index) return null;

            return new RuntimeMorphTargetBinding(targetRenderer, binding.Index, binding.Weight * MorphTargetBinding.VRM_TO_UNITY);
        }

        public SkinnedMeshRenderer TargetRenderer { get; }
        public int TargetRendererInstanceId { get; } // NOTE: Compare key
        public int TargetBlendShapeIndex { get; } // NOTE: Compare key
        public float Weight { get; }

        private RuntimeMorphTargetBinding(SkinnedMeshRenderer targetRenderer, int targetBlendShapeIndex, float weight)
        {
            TargetRenderer = targetRenderer;
            TargetRendererInstanceId = TargetRenderer.GetInstanceID();
            TargetBlendShapeIndex = targetBlendShapeIndex;
            Weight = weight;
        }

        public bool Equals(RuntimeMorphTargetBinding other)
        {
            return TargetRendererInstanceId == other.TargetRendererInstanceId && TargetBlendShapeIndex == other.TargetBlendShapeIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is RuntimeMorphTargetBinding other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TargetRendererInstanceId, TargetBlendShapeIndex);
        }

        public static IEqualityComparer<RuntimeMorphTargetBinding> Comparer { get; } = new EqualityComparer();

        private sealed class EqualityComparer : IEqualityComparer<RuntimeMorphTargetBinding>
        {
            public bool Equals(RuntimeMorphTargetBinding x, RuntimeMorphTargetBinding y)
            {
                return x.TargetRendererInstanceId == y.TargetRendererInstanceId && x.TargetBlendShapeIndex == y.TargetBlendShapeIndex;
            }

            public int GetHashCode(RuntimeMorphTargetBinding obj)
            {
                return HashCode.Combine(obj.TargetRendererInstanceId, obj.TargetBlendShapeIndex);
            }
        }
    }
}