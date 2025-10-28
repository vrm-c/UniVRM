using System;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    internal readonly struct MorphTargetIdentifier : IEquatable<MorphTargetIdentifier>
    {
        public static MorphTargetIdentifier? Create(MorphTargetBinding binding, Transform modelRoot)
        {
            if (modelRoot == null) return null;

            var targetGameObject = modelRoot.Find(binding.RelativePath);
            if (targetGameObject == null) return null;

            var targetRenderer = targetGameObject.GetComponentOrNull<SkinnedMeshRenderer>();
            if (targetRenderer == null) return null;
            if (targetRenderer.sharedMesh == null) return null;
            if (targetRenderer.sharedMesh.blendShapeCount <= binding.Index) return null;

            return new MorphTargetIdentifier(targetRenderer, binding.Index);
        }

        public SkinnedMeshRenderer TargetRenderer { get; }
        public int TargetRendererInstanceId { get; }
        public int TargetBlendShapeIndex { get; }

        public MorphTargetIdentifier(SkinnedMeshRenderer targetRenderer, int targetBlendShapeIndex)
        {
            TargetRenderer = targetRenderer;
            TargetRendererInstanceId = targetRenderer.GetInstanceID();
            TargetBlendShapeIndex = targetBlendShapeIndex;
        }

        public bool Equals(MorphTargetIdentifier other)
        {
            return TargetRendererInstanceId == other.TargetRendererInstanceId && TargetBlendShapeIndex == other.TargetBlendShapeIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is MorphTargetIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TargetRendererInstanceId, TargetBlendShapeIndex);
        }

        #region IEqualityComparer
        public static IEqualityComparer<MorphTargetIdentifier> Comparer { get; } = new EqualityComparer();

        private sealed class EqualityComparer : IEqualityComparer<MorphTargetIdentifier>
        {
            public bool Equals(MorphTargetIdentifier x, MorphTargetIdentifier y)
            {
                return x.TargetRendererInstanceId == y.TargetRendererInstanceId && x.TargetBlendShapeIndex == y.TargetBlendShapeIndex;
            }

            public int GetHashCode(MorphTargetIdentifier obj)
            {
                return HashCode.Combine(obj.TargetRendererInstanceId, obj.TargetBlendShapeIndex);
            }
        }
        #endregion
    }
}