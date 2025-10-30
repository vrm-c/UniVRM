using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Jobs;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// Transformの必要な機能だけを絞り、Blittableに対応させたクラス
    /// </summary>
    [Serializable]
    public readonly struct BlittableTransform
    {
        private readonly float4x3 _localData;
        private readonly float4x4 _globalData;

        public float3 position => GetPosition();
        public quaternion rotation => GetRotation();
        public float3 lossyScale => GetLossyScale();
        public float3 localPosition => _localData.c0.xyz;
        public quaternion localRotation => _localData.c1;
        public float3 localScale => _localData.c2.xyz;
        public float4x4 localToWorldMatrix => GetLocalToWorldMatrix();
        public float4x4 worldToLocalMatrix => math.inverse(localToWorldMatrix);

        public static BlittableTransform FromTransformAccess(TransformAccess transform)
        {
            return new BlittableTransform(
                transform.rotation,
                transform.localPosition,
                transform.localRotation,
                transform.localScale,
                transform.localToWorldMatrix);
        }

        private BlittableTransform(
            quaternion rotation,
            float3 localPosition,
            quaternion localRotation,
            float3 localScale,
            float4x4 localToWorldMatrix)
        {
            var c10 = new float4(localPosition, 0);
            var c11 = localRotation;
            var c12 = new float4(localScale, 0);
            _localData = new float4x3(c10, c11.value, c12);
            
            var c20 = new float4(localToWorldMatrix.c0.xyz, rotation.value.x);
            var c21 = new float4(localToWorldMatrix.c1.xyz, rotation.value.y);
            var c22 = new float4(localToWorldMatrix.c2.xyz, rotation.value.z);
            var c23 = new float4(localToWorldMatrix.c3.xyz, rotation.value.w);
            _globalData = new float4x4(c20, c21, c22, c23);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float4x4 GetLocalToWorldMatrix()
        {
            var c0 = new float4(_globalData.c0.xyz, 0);
            var c1 = new float4(_globalData.c1.xyz, 0);
            var c2 = new float4(_globalData.c2.xyz, 0);
            var c3 = new float4(_globalData.c3.xyz, 1);
            return new float4x4(c0, c1, c2, c3);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float3 GetPosition()
        {
            return localToWorldMatrix.c3.xyz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private quaternion GetRotation()
        {
            return new quaternion(_globalData.c0.w, _globalData.c1.w, _globalData.c2.w, _globalData.c3.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float3 GetLossyScale()
        {
            float4x4 tr = float4x4.TRS(position, rotation, new float3(1.0f));
            float4x4 m = math.mul(math.inverse(tr), localToWorldMatrix);
            return new float3(m.c0.x, m.c1.y, m.c2.z);
        }

        /// <summary>
        /// 親Transformが移動や回転を行った際に、ローカル座標系の値をもとに新しい絶対座標系でのTransformを計算します。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public BlittableTransform UpdateParentMatrix(BlittableTransform parent)
        {
            quaternion newRotation = math.mul(parent.rotation, localRotation);
            float4x4 newLocalToWorldMatrix = math.mul(parent.localToWorldMatrix, float4x4.TRS(localPosition, localRotation, localScale));

            return new BlittableTransform(newRotation, localPosition, localRotation, localScale, newLocalToWorldMatrix);
        }

        /// <summary>
        /// グローバル座標系での位置を更新し、それに伴って連動する他の値も更新します。
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public BlittableTransform UpdatePosition(float3 newValue, BlittableTransform? parent = null)
        {
            quaternion newRotation;
            float3 newLocalPosition;
            quaternion newLocalRotation;
            float4x4 newLocalToWorldMatrix;

            if(parent.HasValue)
            {
                newLocalPosition = math.transform(parent.Value.worldToLocalMatrix, newValue);
                newLocalRotation = math.mul(math.inverse(parent.Value.rotation), rotation);
                newRotation = math.mul(parent.Value.rotation, newLocalRotation);
                newLocalToWorldMatrix = math.mul(parent.Value.localToWorldMatrix, float4x4.TRS(newLocalPosition, newLocalRotation, localScale));
            }
            else
            {
                newLocalPosition = newValue;
                newLocalRotation = rotation;
                newRotation = rotation;
                newLocalToWorldMatrix = float4x4.TRS(newValue, newRotation, localScale);
            }

            return new BlittableTransform(newRotation, newLocalPosition, newLocalRotation, localScale, newLocalToWorldMatrix);
        }

        /// <summary>
        /// グローバル座標系での回転を更新し、それに伴って連動する他の値も更新します。
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public BlittableTransform UpdateRotation(quaternion newValue, BlittableTransform? parent = null)
        {
            quaternion newRotation = newValue;
            quaternion newLocalRotation;
            float4x4 newLocalToWorldMatrix;

            if(parent.HasValue)
            {
                newLocalRotation = math.normalize(math.mul(math.inverse(parent.Value.rotation), newRotation));
                newLocalToWorldMatrix = math.mul(parent.Value.localToWorldMatrix, float4x4.TRS(localPosition, newLocalRotation, localScale));
            }
            else
            {
                newLocalRotation = newValue;
                newLocalToWorldMatrix = float4x4.TRS(position, newRotation, localScale);
            }

            return new BlittableTransform(newRotation, localPosition, newLocalRotation, localScale, newLocalToWorldMatrix);
        }
    }
}
