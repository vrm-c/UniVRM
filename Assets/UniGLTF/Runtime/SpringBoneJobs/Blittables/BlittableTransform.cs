using System;
using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs.Blittables
{
    /// <summary>
    /// Transformの必要な機能だけを絞り、Blittableに対応させたクラス
    /// </summary>
    [Serializable]
    public readonly struct BlittableTransform
    {
        public readonly float3 position;
        public readonly quaternion rotation;
        public readonly float3 localPosition;
        public readonly quaternion localRotation;
        public readonly float3 localScale;
        public readonly float4x4 localToWorldMatrix;
        public readonly float4x4 worldToLocalMatrix;
        public float3 lossyScale
        {
            get
            {
                float4x4 tr = float4x4.TRS(position, rotation, new float3(1.0f, 1.0f, 1.0f));
                float4x4 m = math.mul(math.inverse(tr), localToWorldMatrix);
                return new float3(m.c0.x, m.c1.y, m.c2.z);
            }
        }
        
        public BlittableTransform(
            float3 position = default,
            quaternion rotation = default,
            float3 localPosition = default,
            quaternion localRotation = default,
            float3 localScale = default,
            float4x4 localToWorldMatrix = default,
            float4x4 worldToLocalMatrix = default)
        {
            this.position = position;
            this.rotation = rotation;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
            this.localToWorldMatrix = localToWorldMatrix;
            this.worldToLocalMatrix = worldToLocalMatrix;
        }
        
        public BlittableTransform SetPosition(float3 newPosition)
        {
            return new BlittableTransform(
                position: newPosition,
                rotation: rotation,
                localPosition: localPosition,
                localRotation: localRotation,
                localScale: localScale,
                localToWorldMatrix: localToWorldMatrix,
                worldToLocalMatrix: worldToLocalMatrix);
        }
        
        public BlittableTransform SetRotation(quaternion newRotation)
        {
            return new BlittableTransform(
                position: position,
                rotation: newRotation,
                localPosition: localPosition,
                localRotation: localRotation,
                localScale: localScale,
                localToWorldMatrix: localToWorldMatrix,
                worldToLocalMatrix: worldToLocalMatrix);
        }
        
        public BlittableTransform SetLocalPosition(float3 newLocalPosition)
        {
            return new BlittableTransform(
                position: position,
                rotation: rotation,
                localPosition: newLocalPosition,
                localRotation: localRotation,
                localScale: localScale,
                localToWorldMatrix: localToWorldMatrix,
                worldToLocalMatrix: worldToLocalMatrix);
        }
        
        public BlittableTransform SetLocalRotation(quaternion newLocalRotation)
        {
            return new BlittableTransform(
                position: position,
                rotation: rotation,
                localPosition: localPosition,
                localRotation: newLocalRotation,
                localScale: localScale,
                localToWorldMatrix: localToWorldMatrix,
                worldToLocalMatrix: worldToLocalMatrix);
        }
        
        public BlittableTransform SetLocalScale(float3 newLocalScale)
        {
            return new BlittableTransform(
                position: position,
                rotation: rotation,
                localPosition: localPosition,
                localRotation: localRotation,
                localScale: newLocalScale,
                localToWorldMatrix: localToWorldMatrix,
                worldToLocalMatrix: worldToLocalMatrix);
        }
        
        public BlittableTransform SetLocalToWorldMatrix(float4x4 newLocalToWorldMatrix)
        {
            return new BlittableTransform(
                position: position,
                rotation: rotation,
                localPosition: localPosition,
                localRotation: localRotation,
                localScale: localScale,
                localToWorldMatrix: newLocalToWorldMatrix,
                worldToLocalMatrix: math.inverse(newLocalToWorldMatrix));
        }
        
        public BlittableTransform SetWorldToLocalMatrix(float4x4 newWorldToLocalMatrix)
        {
            return new BlittableTransform(
                position: position,
                rotation: rotation,
                localPosition: localPosition,
                localRotation: localRotation,
                localScale: localScale,
                localToWorldMatrix: math.inverse(newWorldToLocalMatrix),
                worldToLocalMatrix: newWorldToLocalMatrix);
        }
    }
}