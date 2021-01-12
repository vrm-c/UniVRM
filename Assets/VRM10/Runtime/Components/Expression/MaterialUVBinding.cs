using System;
using UnityEngine;

namespace UniVRM10
{
    /// <summary>
    /// 対象のマテリアルのすべての テクスチャの Scale/Offset をまとめて変更する。
    /// 主に _MainTex_ST 値
    /// </summary>
    [Serializable]
    public struct MaterialUVBinding : IEquatable<MaterialUVBinding>
    {
        /// <summary>
        /// モデルのヒエラルキーから得たマテリアルの中から名前で検索する。
        /// 同名マテリアルはやめてください。
        /// </summary>
        public String MaterialName;

        public Vector2 Scaling; // default: Vector2.one
        public Vector2 Offset; // default: Vector2.zero

        public Vector4 ScalingOffset => new Vector4(Scaling.x, Scaling.y, Offset.x, Offset.y);

        public bool Equals(MaterialUVBinding other)
        {
            return string.Equals(MaterialName, other.MaterialName) && Scaling.Equals(other.Scaling) && Offset.Equals(other.Offset);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MaterialUVBinding && Equals((MaterialUVBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (MaterialName != null ? MaterialName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Scaling.GetHashCode();
                hashCode = (hashCode * 397) ^ Offset.GetHashCode();
                return hashCode;
            }
        }
    }
}
