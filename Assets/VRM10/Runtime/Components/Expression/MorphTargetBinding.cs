using System;
using UniGLTF;
using UnityEngine;

namespace UniVRM10
{
    [Serializable]
    public struct MorphTargetBinding : IEquatable<MorphTargetBinding>
    {
        /// <summary>
        /// Unity の BlendShape の値域は 0-100
        /// </summary>
        public const float VRM_TO_UNITY = 100.0f;

        /// <summary>
        /// VRM-1.0 の MorphTargetBinding.Weight の値域は 0-1.0
        /// </summary>
        public const float UNITY_TO_VRM = 0.01f;

        public const float MAX_WEIGHT = 1.0f;

        /// <summary>
        /// SkinnedMeshRenderer.BlendShape[Index].Weight を 指し示す。
        /// 
        /// [トレードオフ]
        /// 
        /// * SkinnedMeshRenderer そのものの方がわかりやすい
        /// * Prefab 生成時の順番問題
        ///   * Prefabの中で、Prefab自体を参照するので String の方が扱いが楽(Prefab生成時にトラブルになりがち。Runtimeロードでは問題ない)
        /// * モデル変更時の改変への強さ
        /// 
        /// </summary>

        // シリアライズしてEditorから変更するので readonly 不可
        public /*readonly*/ String RelativePath;
        public /*readonly*/ int Index;
        public /*readonly*/ float Weight;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="index"></param>
        /// <param name="weight">0 to 1.0</param>
        public MorphTargetBinding(string path, int index, float weight)
        {
            RelativePath = path;
            Index = index;
            if (weight > MAX_WEIGHT)
            {
                UniGLTFLogger.Warning($"MorphTargetBinding: {weight} > {MAX_WEIGHT}");
            }
            Weight = weight;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]=>{2}", RelativePath, Index, Weight);
        }

        public bool Equals(MorphTargetBinding other)
        {
            return string.Equals(RelativePath, other.RelativePath) && Index == other.Index && Weight.Equals(other.Weight);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MorphTargetBinding && Equals((MorphTargetBinding)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (RelativePath != null ? RelativePath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Index;
                hashCode = (hashCode * 397) ^ Weight.GetHashCode();
                return hashCode;
            }
        }
    }
}
