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
#if UNITY_6000_5_OR_NEWER
        public EntityId TargetRendererEntityId { get; }
#else
        public int TargetRendererInstanceId { get; }
#endif
        public int TargetBlendShapeIndex { get; }

        public MorphTargetIdentifier(SkinnedMeshRenderer targetRenderer, int targetBlendShapeIndex)
        {
            TargetRenderer = targetRenderer;
#if UNITY_6000_5_OR_NEWER
            TargetRendererEntityId = targetRenderer.GetEntityId();
#else
            TargetRendererInstanceId = targetRenderer.GetInstanceID();
#endif
            TargetBlendShapeIndex = targetBlendShapeIndex;
        }

        public bool Equals(MorphTargetIdentifier other)
        {
#if UNITY_6000_5_OR_NEWER
            return TargetRendererEntityId == other.TargetRendererEntityId && TargetBlendShapeIndex == other.TargetBlendShapeIndex;
#else
            return TargetRendererInstanceId == other.TargetRendererInstanceId && TargetBlendShapeIndex == other.TargetBlendShapeIndex;
#endif
        }

        public override bool Equals(object obj)
        {
            return obj is MorphTargetIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
#if UNITY_6000_5_OR_NEWER
            return HashCode.Combine(TargetRendererEntityId, TargetBlendShapeIndex);
#else
            return HashCode.Combine(TargetRendererInstanceId, TargetBlendShapeIndex);
#endif
        }

        #region IEqualityComparer
        public static IEqualityComparer<MorphTargetIdentifier> Comparer { get; } = new EqualityComparer();

        private sealed class EqualityComparer : IEqualityComparer<MorphTargetIdentifier>
        {
            public bool Equals(MorphTargetIdentifier x, MorphTargetIdentifier y)
            {
#if UNITY_6000_5_OR_NEWER
                return x.TargetRendererEntityId == y.TargetRendererEntityId && x.TargetBlendShapeIndex == y.TargetBlendShapeIndex;
#else
                return x.TargetRendererInstanceId == y.TargetRendererInstanceId && x.TargetBlendShapeIndex == y.TargetBlendShapeIndex;
#endif
            }

            public int GetHashCode(MorphTargetIdentifier obj)
            {
#if UNITY_6000_5_OR_NEWER
                return HashCode.Combine(obj.TargetRendererEntityId, obj.TargetBlendShapeIndex);
#else
                return HashCode.Combine(obj.TargetRendererInstanceId, obj.TargetBlendShapeIndex);
#endif
            }
        }
        #endregion
    }
}