using System;
using System.Collections.Generic;
#if ENABLE_SPRINGBONE_BURST
using Unity.Burst;
#endif
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using VRM.FastSpringBones.Blittables;
using VRM.FastSpringBones.NativeWrappers;
using VRM.FastSpringBones.Registries;

namespace VRM.FastSpringBones.Schedulers
{
    /// <summary>
    /// Blittableな世界からGameObjectの世界へTransformを送り込む処理を行うクラス
    /// </summary>
    public sealed unsafe class PushTransformJobScheduler : IDisposable
    {
        private BlittableTransform** _transformPointers;
        private TransformAccessArray _transformAccessArray;

        private readonly CustomSampler _sampler = CustomSampler.Create("Schedule CopyFromTransformJob");
        private readonly TransformRegistry _transformRegistry;

        private bool _dirty = true;

        private IReadOnlyList<NativeTransform> Targets => _transformRegistry.PushTargets;

        public PushTransformJobScheduler(TransformRegistry transformRegistry)
        {
            _transformRegistry = transformRegistry;

            _transformRegistry.SubscribeOnValueChanged(OnTransformChanged);
        }

        private void OnTransformChanged()
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
            var job = new Job { TransformPointers = _transformPointers };
            var jobHandle = job.Schedule(_transformAccessArray, dependOn);

            _sampler.End();

            return jobHandle;
        }

        private void ReconstructBuffers()
        {
            ReleaseBuffers();

            var transforms = Targets;
            _transformPointers = (BlittableTransform**)UnsafeUtility.Malloc(
                sizeof(BlittableTransform*) * transforms.Count,
                16,
                Allocator.Persistent
            );

            _transformAccessArray = new TransformAccessArray(transforms.Count);

            for (var i = 0; i < transforms.Count; i++)
            {
                _transformPointers[i] = transforms[i].GetUnsafePtr();
                _transformAccessArray.Add(transforms[i].Transform);
            }
        }

        public void Dispose()
        {
            ReleaseBuffers();
            _transformRegistry.UnSubscribeOnValueChanged(OnTransformChanged);
        }

        private void ReleaseBuffers()
        {
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_transformPointers != null)
            {
                UnsafeUtility.Free(_transformPointers, Allocator.Persistent);
                _transformPointers = null;
            }
        }

#if ENABLE_SPRINGBONE_BURST
        [BurstCompile]
#endif
        private struct Job : IJobParallelForTransform
        {
            [NativeDisableUnsafePtrRestriction] public BlittableTransform** TransformPointers;

            public void Execute(int index, TransformAccess transform)
            {
                TransformPointers[index]->PushTo(transform);
            }
        }
    }
}
