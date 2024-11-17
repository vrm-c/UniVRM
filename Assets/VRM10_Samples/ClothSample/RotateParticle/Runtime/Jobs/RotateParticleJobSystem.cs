using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RotateParticle.Components;
using UniGLTF;
using UniGLTF.SpringBoneJobs.Blittables;
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

        NativeArray<WarpInfo> _warps;

        TransformAccessArray _transformAccessArray;
        NativeArray<ParticleInfo> _particles;
        NativeArray<Vector3> _prevPositionsOnCenter;
        NativeArray<Vector3> _currentPositions;
        NativeArray<Vector3> _nextPositions;
        NativeArray<Quaternion> _currentRotations;
        NativeArray<Quaternion> _nextRotations;

        void IDisposable.Dispose()
        {
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_warps.IsCreated) _warps.Dispose();
            if (_particles.IsCreated) _particles.Dispose();
            if (_prevPositionsOnCenter.IsCreated) _prevPositionsOnCenter.Dispose();
            if (_currentPositions.IsCreated) _currentPositions.Dispose();
            if (_nextPositions.IsCreated) _nextPositions.Dispose();
        }

        async Task IRotateParticleSystem.InitializeAsync(Vrm10Instance vrm, IAwaitCaller awaitCaller)
        {
            _vrm = vrm;

            List<Transform> transforms = new();
            var warps = vrm.GetComponentsInChildren<Warp>();
            foreach(var warp in warps)
            {
                
            }
            await awaitCaller.NextFrame();
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
            handle = new LoadTransformJob
            {
                CurrentPositions = _currentPositions,
                CurrentRotations = _currentRotations,
            }.Schedule(_transformAccessArray, handle);

            // verlet
            handle = new VerletJob
            {
                Frame = frame,
                Warps = _warps,
                Particles = _particles,
                PrevPositionsOnCenter = _prevPositionsOnCenter,
                CurrentPositions = _currentPositions,
                NextPositions = _nextPositions,
            }.Schedule(_currentPositions.Length, 128, handle);

            // local rotation constraint
            handle = new LocalRotationConstraintJob
            {
            }.Schedule(_warps.Length, 1, handle);

            // update verlet

            // writeback transform

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