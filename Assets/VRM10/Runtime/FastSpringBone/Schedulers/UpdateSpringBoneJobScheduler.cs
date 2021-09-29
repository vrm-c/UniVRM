using System;
using System.Collections.Generic;
#if ENABLE_SPRINGBONE_BURST
using Unity.Burst;
#endif
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using VRM.FastSpringBones.Blittables;
using VRM.FastSpringBones.NativeWrappers;
using VRM.FastSpringBones.Registries;

namespace VRM.FastSpringBones.Schedulers
{
    /// <summary>
    /// SpringBoneを更新する処理を行うクラス
    /// </summary>
    public sealed unsafe class UpdateSpringBoneJobScheduler : IDisposable
    {
        private BlittableRootBone** _rootBonePointers;

        private readonly RootBoneRegistry _rootBoneRegistry;
        private readonly CustomSampler _sampler = CustomSampler.Create("Schedule CopyFromTransformJob");

        private bool _dirty = true;

        private IReadOnlyList<NativePointer<BlittableRootBone>> Targets => _rootBoneRegistry.Items;

        public UpdateSpringBoneJobScheduler(RootBoneRegistry rootBoneRegistry)
        {
            _rootBoneRegistry = rootBoneRegistry;

            _rootBoneRegistry.SubscribeOnValueChanged(OnRootBoneChanged);
        }

        public void Dispose()
        {
            ReleaseBuffer();
            _rootBoneRegistry.UnSubscribeOnValueChanged(OnRootBoneChanged);
        }

        private void OnRootBoneChanged()
        {
            _dirty = true;
        }

        public JobHandle Schedule(JobHandle dependOn = default(JobHandle))
        {
            if (Targets.Count == 0)
            {
                return dependOn;
            }

            _sampler.Begin();

            // リストが変更されていたらバッファを再構築
            if (_dirty)
            {
                ReconstructBuffers();

                _dirty = false;
            }

            // Jobを発火
            var job = new Job { RootBonePointers = _rootBonePointers, DeltaTime = Time.deltaTime };
            var jobHandle = job.Schedule(Targets.Count, 0, dependOn);

            _sampler.End();

            return jobHandle;
        }

        private void ReconstructBuffers()
        {
            ReleaseBuffer();

            _rootBonePointers = (BlittableRootBone**)UnsafeUtility.Malloc(
                sizeof(BlittableTransform*) * Targets.Count,
                16,
                Allocator.Persistent
            );

            for (var i = 0; i < Targets.Count; i++)
            {
                _rootBonePointers[i] = Targets[i].GetUnsafePtr();
            }
        }

        private void ReleaseBuffer()
        {
            if (_rootBonePointers == null) return;
            UnsafeUtility.Free(_rootBonePointers, Allocator.Persistent);
            _rootBonePointers = null;
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
        private struct Job : IJobParallelFor
        {
            [NativeDisableUnsafePtrRestriction] public BlittableRootBone** RootBonePointers;
            public float DeltaTime;

            public void Execute(int index)
            {
                // 各点を更新する
                RootBonePointers[index]->Update(DeltaTime);
            }
        }
    }
}
