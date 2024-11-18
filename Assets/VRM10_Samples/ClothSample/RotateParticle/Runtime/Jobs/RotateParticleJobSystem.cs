using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RotateParticle.Components;
using UniGLTF;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UniVRM10;


namespace RotateParticle.Jobs
{
    public class RotateParticleJobSystem : IRotateParticleSystem
    {
        Vrm10Instance _vrm;

        NativeArray<WarpImmutable> _warpImmutables;
        NativeArray<WarpMutable> _warpMutables;

        TransformAccessArray _transformAccessArray;
        NativeArray<ParticleImmutable> _particleImmutables;
        NativeArray<ParticleMutable> _particleMutables;

        NativeArray<Vector3> _prevPositionsOnCenter;
        NativeArray<Vector3> _currentPositions;
        NativeArray<Vector3> _nextPositions;
        // NativeArray<Quaternion> _currentRotations;
        NativeArray<Quaternion> _nextRotations;

        bool _zeroVelocity = true;

        void IDisposable.Dispose()
        {
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_warpImmutables.IsCreated) _warpImmutables.Dispose();

            if (_warpMutables.IsCreated) _warpMutables.Dispose();
            if (_particleImmutables.IsCreated) _particleImmutables.Dispose();
            if (_particleMutables.IsCreated) _particleMutables.Dispose();

            if (_prevPositionsOnCenter.IsCreated) _prevPositionsOnCenter.Dispose();
            if (_currentPositions.IsCreated) _currentPositions.Dispose();
            if (_nextPositions.IsCreated) _nextPositions.Dispose();
            // if (_currentRotations.IsCreated) _currentRotations.Dispose();
            if (_nextRotations.IsCreated) _nextRotations.Dispose();
        }

        async Task IRotateParticleSystem.InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _vrm = vrm;

            List<WarpImmutable> warpImmutables = new();
            List<WarpMutable> warpMutables = new();

            List<Transform> transforms = new();
            List<ParticleImmutable> particleImmutables = new();
            // List<ParticleMutable> particleMutables = new();
            var warpSrcs = vrm.GetComponentsInChildren<Warp>();
            for (int warpIndex = 0; warpIndex < warpSrcs.Length; ++warpIndex)
            {
                var warp = warpSrcs[warpIndex];
                warpImmutables.Add(new WarpImmutable
                {

                });
                warpMutables.Add(new WarpMutable
                {

                });

                transforms.Add(warp.transform);
                particleImmutables.Add(new ParticleImmutable
                {
                    ParticleType = ParticleType.Root,
                    WarpIndex = warpIndex,
                });
                foreach (var particle in warp.Particles)
                {
                    if (particle != null && particle.Transform != null)
                    {
                        transforms.Add(particle.Transform);
                        particleImmutables.Add(new ParticleImmutable
                        {
                            // TODO: ParticleType.Branch
                            ParticleType = ParticleType.Verlet,
                            WarpIndex = warpIndex,
                        });
                    }
                }
                await awaitCaller.NextFrame();
            }

            _warpImmutables = new(warpImmutables.ToArray(), Allocator.Persistent);
            _warpMutables = new(warpMutables.ToArray(), Allocator.Persistent);

            _transformAccessArray = new TransformAccessArray(transforms.ToArray());
            _particleImmutables = new(particleImmutables.ToArray(), Allocator.Persistent);
            _particleMutables = new(transforms.Count, Allocator.Persistent);

            _currentPositions = new(transforms.Count, Allocator.Persistent);
            _prevPositionsOnCenter = new(transforms.Count, Allocator.Persistent);
            _nextPositions = new(transforms.Count, Allocator.Persistent);
            // _currentRotations = new(transforms.Count, Allocator.Persistent);
            _nextRotations = new(transforms.Count, Allocator.Persistent);
        }

        void IRotateParticleSystem.Process(float deltaTime)
        {
            Schedule(
                new FrameInfo(deltaTime, Vector3.zero),
                _transformAccessArray
            ).Complete();
        }

        /// <summary>
        /// INIT        PROCESS
        ///             _transformAccessArray
        ///             👇
        ///             _currentPositions
        ///             _currentRotations
        /// </summary>
        private JobHandle Schedule(
            in FrameInfo frame,
            TransformAccessArray transformAccessArray
        )
        {
            JobHandle handle = default;

            // load transform
            handle = new ReadTransformJob
            {
                CurrentPositions = _currentPositions,
                NextRotations = _nextRotations,
            }.Schedule(_transformAccessArray, handle);

            // verlet
            handle = new VerletJob
            {
                ZeroVelocity = _zeroVelocity,
                Frame = frame,
                WarpMutables = _warpMutables,
                ParticleImmutables = _particleImmutables,
                ParticleMutables = _particleMutables,
                PrevPositionsOnCenter = _prevPositionsOnCenter,
                CurrentPositions = _currentPositions,
                NextPositions = _nextPositions,
            }.Schedule(_currentPositions.Length, 128, handle);
            _zeroVelocity = false;

            // local rotation constraint
            handle = new LocalRotationConstraintJob
            {
                WarpImmutables = _warpImmutables,
                ParticleImmutables = _particleImmutables,
                Positions = _nextPositions,
                Rotations = _nextRotations,
            }.Schedule(_warpImmutables.Length, 1, handle);

            // update verlet
            handle = new VerletStatusJob
            {
                WarpImmutables = _warpImmutables,
                WarpMutables = _warpMutables,
                CurrentPositions = _currentPositions,
                PrevPositionsOnCenter = _prevPositionsOnCenter,
            }.Schedule(_warpImmutables.Length, 1, handle);

            // writeback transform
            handle = new WritebackTransformJob
            {
                NextPositions = _nextPositions,
                NextRotations = _nextRotations,
            }.Schedule(_transformAccessArray, handle);

            return handle;
        }

        void IRotateParticleSystem.ResetInitialRotation()
        {
            throw new NotImplementedException();
        }

        void IRotateParticleSystem.DrawGizmos()
        {
        }
    }
}