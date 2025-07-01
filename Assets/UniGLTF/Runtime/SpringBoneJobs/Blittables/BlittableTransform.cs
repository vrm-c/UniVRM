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
        public readonly float3 _position;
        public readonly quaternion _rotation;
        public readonly float3 _localPosition;
        public readonly quaternion _localRotation;
        public readonly float3 _localScale;
        public readonly float4x4 _localToWorldMatrix;

        public float3 position => _position;
        public quaternion rotation => _rotation;
        public float3 lossyScale => GetLossyScale();
        public float3 localPosition => _localPosition;
        public quaternion localRotation => _localRotation;
        public float3 localScale => _localScale;
        public float4x4 localToWorldMatrix => _localToWorldMatrix;
        public float4x4 worldToLocalMatrix => math.inverse(_localToWorldMatrix);

        public BlittableTransform(TransformAccess transform)
        {
            _position = transform.position;
            _rotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
            _localScale = transform.localScale;
            _localToWorldMatrix = transform.localToWorldMatrix;
        }

        private BlittableTransform(
            float3 position,
            quaternion rotation,
            float3 localPosition,
            quaternion localRotation,
            float3 localScale,
            float4x4 localToWorldMatrix)
        {
            _position = position;
            _rotation = rotation;
            _localPosition = localPosition;
            _localRotation = localRotation;
            _localScale = localScale;
            _localToWorldMatrix = localToWorldMatrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float3 GetLossyScale()
        {
            float4x4 tr = float4x4.TRS(position, rotation, new float3(1.0f));
            float4x4 m = math.mul(math.inverse(tr), _localToWorldMatrix);
            return new float3(m.c0.x, m.c1.y, m.c2.z);
        }

        /// <summary>
        /// 親Transformが移動や回転を行った際に、ローカル座標系の値をもとに新しい絶対座標系でのTransformを計算します。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public BlittableTransform UpdateParentMatrix(BlittableTransform parent)
        {
            float3 newPosition = math.mul(parent.localToWorldMatrix, new float4(localPosition, 1)).xyz;
            quaternion newRotation = math.mul(parent.rotation, localRotation);
            float3 newLocalPosition = _localPosition;
            quaternion newLocalRotation = _localRotation;
            float3 newLocalScale = _localScale;
            float4x4 newLocalToWorldMatrix = math.mul(parent.localToWorldMatrix, float4x4.TRS(newLocalPosition, newLocalRotation, newLocalScale));

            return new BlittableTransform(newPosition, newRotation, newLocalPosition, newLocalRotation, newLocalScale, newLocalToWorldMatrix);
        }

        /// <summary>
        /// グローバル座標系での位置を更新し、それに伴って連動する他の値も更新します。
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public BlittableTransform UpdatePosition(float3 newValue, BlittableTransform? parent = null)
        {
            float3 newPosition = newValue;
            quaternion newRotation = _rotation;
            float3 newLocalPosition = _localPosition;
            quaternion newLocalRotation = _localRotation;
            float3 newLocalScale = _localScale;
            float4x4 newLocalToWorldMatrix = _localToWorldMatrix;

            if(parent.HasValue)
            {
                newLocalPosition = math.transform(parent.Value.worldToLocalMatrix, newValue);
                newLocalRotation = math.mul(math.inverse(parent.Value.rotation), _rotation);
                newPosition = math.transform(parent.Value.localToWorldMatrix, newLocalPosition);
                newRotation = math.mul(parent.Value.rotation, newLocalRotation);
                newLocalToWorldMatrix = math.mul(parent.Value.localToWorldMatrix, float4x4.TRS(newLocalPosition, newLocalRotation, newLocalScale));
            }
            else
            {
                newLocalPosition = newValue;
                newPosition = newValue;
                newLocalRotation = _rotation;
                newRotation = _rotation;
                newLocalToWorldMatrix = float4x4.TRS(newPosition, newRotation, newLocalScale);
            }

            return new BlittableTransform(newPosition, newRotation, newLocalPosition, newLocalRotation, newLocalScale, newLocalToWorldMatrix);
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
            float3 newPosition = _position;
            float3 newLocalPosition = _localPosition;
            quaternion newLocalRotation = _localRotation;
            float3 newLocalScale = _localScale;
            float4x4 newLocalToWorldMatrix = _localToWorldMatrix;

            if(parent.HasValue)
            {
                newLocalRotation = math.normalize(math.mul(math.inverse(parent.Value.rotation), newRotation));
                newLocalToWorldMatrix = math.mul(parent.Value.localToWorldMatrix, float4x4.TRS(_localPosition, newLocalRotation, _localScale));
            }
            else
            {
                newLocalRotation = newValue;
                newLocalToWorldMatrix = float4x4.TRS(newPosition, newRotation, newLocalScale);
            }

            return new BlittableTransform(newPosition, newRotation, newLocalPosition, newLocalRotation, newLocalScale, newLocalToWorldMatrix);
        }
    }
}
