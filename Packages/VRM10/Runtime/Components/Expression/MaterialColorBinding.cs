using System;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public struct MaterialColorBinding : IEquatable<MaterialColorBinding>
    {
        public String MaterialName;
        public UniGLTF.Extensions.VRMC_vrm.MaterialColorType BindType;
        public Vector4 TargetValue;

        public bool Equals(MaterialColorBinding other)
        {
            return string.Equals(MaterialName, other.MaterialName) && BindType.Equals(other.BindType) && TargetValue.Equals(other.TargetValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MaterialColorBinding && Equals((MaterialColorBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (MaterialName != null ? MaterialName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ BindType.GetHashCode();
                hashCode = (hashCode * 397) ^ TargetValue.GetHashCode();
                return hashCode;
            }
        }
    }
}
