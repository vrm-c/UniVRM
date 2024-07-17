using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    /// <summary>
    /// ひとつのVRMに紐づくFastSpringBoneに関連したバッファを保持するクラス
    /// </summary>
    public class FastSpringBoneBuffer : IDisposable
    {
        // NOTE: これらはFastSpringBoneBufferCombinerによってバッチングされる
        public NativeArray<BlittableSpring> Springs { get; }
        public NativeArray<BlittableJoint> Joints { get; }
        public NativeArray<BlittableCollider> Colliders { get; }
        public NativeArray<BlittableLogic> Logics { get; }
        public NativeArray<BlittableTransform> BlittableTransforms { get; }
        public Transform[] Transforms { get; }
        public bool IsDisposed { get; private set; }

        // NOTE: これは更新頻度が高くバッチングが難しいため、ランダムアクセスを許容してメモリへ直接アクセスする
        // 生のヒープ領域は扱いにくいので長さ1のNativeArrayで代用
        private NativeArray<BlittableExternalData> _externalData;
        public Vector3 ExternalForce
        {
            get => _externalData[0].ExternalForce;
            set
            {
                _externalData[0] = new BlittableExternalData
                {
                    ExternalForce = value,
                    IsSpringBoneEnabled = _externalData[0].IsSpringBoneEnabled,
                };
            }
        }
        public bool IsSpringBoneEnabled
        {
            get => _externalData[0].IsSpringBoneEnabled;
            set
            {
                _externalData[0] = new BlittableExternalData
                {
                    ExternalForce = _externalData[0].ExternalForce,
                    IsSpringBoneEnabled = value,
                };
            }

        }

        public unsafe FastSpringBoneBuffer(FastSpringBoneBufferBuilder b)
        {
            Profiler.BeginSample("FastSpringBone.ConstructBuffers.ConstructNativeArrays");
            _externalData = new NativeArray<BlittableExternalData>(1, Allocator.Persistent);
            _externalData[0] = new BlittableExternalData
            {
                ExternalForce = Vector3.zero,
                IsSpringBoneEnabled = true,
            };
            b.SetExternalDataPtr((BlittableExternalData*)_externalData.GetUnsafePtr());
            Springs = new NativeArray<BlittableSpring>(b.BlittableSprings.ToArray(), Allocator.Persistent);
            Joints = new NativeArray<BlittableJoint>(b.BlittableJoints.ToArray(), Allocator.Persistent);
            Colliders = new NativeArray<BlittableCollider>(b.BlittableColliders.ToArray(), Allocator.Persistent);
            Logics = new NativeArray<BlittableLogic>(b.BlittableLogics.ToArray(), Allocator.Persistent);
            BlittableTransforms = new NativeArray<BlittableTransform>(b.Transforms.Length, Allocator.Persistent);
            Transforms = b.Transforms;
            Profiler.EndSample();
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Springs.Dispose();
            Joints.Dispose();
            BlittableTransforms.Dispose();
            Colliders.Dispose();
            Logics.Dispose();
            _externalData.Dispose();
        }
    }
}