using System;
using System.Collections.Generic;
using RotateParticle.Components;
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
        readonly Vrm10Instance _vrm;
        TransformAccessArray _transformAccessArray;
        NativeArray<BlittableTransform> _transforms;

        enum ParticleType
        {
            // kinematic. not verlet move
            Root,
            // particle. verlet move
            Verlet,
            // branch. move same as 1st branch sibling
            Branch,
        }

        struct ParticleInfo
        {
            public ParticleType ParticleType;
            public int WarpIndex;
            public int ParentIndex;
            public float DistanceFromParent;
            public Quaternion InitRotation;
            public Vector3 InitTailLocalPosition;
        }

        struct WarpInfo
        {
            public Vector3 Center;
            public float DragForce;
            public Quaternion ParentRotation;
            public int BranchRootIndex;
            public int Start;
            public int End;
        }
        NativeArray<WarpInfo> _warps;

        NativeArray<ParticleInfo> _particles;
        NativeArray<Vector3> _verletPrevOnCenter;
        NativeArray<Vector3> _verletCurrent;
        NativeArray<Vector3> _verletNext;

        public IList<Warp> Warps => throw new NotImplementedException();

        public IList<RectCloth> Cloths => throw new NotImplementedException();

        struct VerletJob : IJobParallelFor
        {
            [ReadOnly] public FrameInfo Frame;

            [ReadOnly] public NativeArray<WarpInfo> Warps;


            [ReadOnly] public NativeArray<ParticleInfo> Particles;
            [ReadOnly] public NativeArray<Vector3> PrevPositionsInCenter;
            [ReadOnly] public NativeArray<Vector3> CurrentPositions;
            [WriteOnly] public NativeArray<Vector3> NextPositions;
            public void Execute(int index)
            {
                var particle = Particles[index];
                if (particle.ParticleType == ParticleType.Root)
                {
                    // kinematic. not move
                    NextPositions[index] = CurrentPositions[index];
                }
                else
                {
                    var warp = Warps[particle.WarpIndex];
                    var velocity = CurrentPositions[index] - warp.Center - PrevPositionsInCenter[index];
                    velocity *= 1 - warp.DragForce;
                    NextPositions[index] = CurrentPositions[index] +
                        velocity + Frame.Force * Frame.SqDeltaTime;
                }
            }
        }

        /// <summary>
        /// Warp毎に verlet の結果を拘束する
        /// 1. 親子間の距離を一定に保つ
        /// 2. 移動を親の回転に変換する
        /// </summary>
        struct LocalRotationConstraintJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<WarpInfo> Warps;
            [ReadOnly] public NativeArray<ParticleInfo> Particles;
            [NativeDisableParallelForRestriction] public NativeArray<Vector3> Positions;
            [NativeDisableParallelForRestriction] public NativeArray<Quaternion> Rotations;

            public void Execute(int index)
            {
                var warp = Warps[index];
                for (int i = warp.Start; i < warp.End; ++i)
                {
                    var particle = Particles[i];
                    switch (particle.ParticleType)
                    {
                        case ParticleType.Root:
                            break;

                        case ParticleType.Verlet:
                            if (i + 1 < warp.End)
                            {
                                var next = Particles[i + 1];
                                if (next.ParentIndex == i)
                                {
                                    /// 1. 親子間の距離を一定に保つ
                                    Positions[i + 1] = CalcPosition(Positions[i + 1],
                                        Positions[i], next.DistanceFromParent);
                                    // 2. 移動を親の回転に変換する
                                    var rest = Rotations[i] * next.InitRotation;
                                    Rotations[i] = rest * Quaternion.FromToRotation(
                                        // 初期状態
                                        rest * particle.InitTailLocalPosition,
                                        // 現状
                                        Positions[i + 1] - Positions[i]
                                    );
                                }
                            }
                            break;

                        case ParticleType.Branch:
                            {
                                // TODO
                                break;
                            }
                    }
                }
            }

            static Vector3 CalcPosition(in Vector3 pos, in Vector3 parent, float distance)
            {
                return pos + (pos - parent).normalized * distance;
            }
        }

        public RotateParticleJobSystem(Vrm10Instance vrm)
        {
            _vrm = vrm;
            var warps = vrm.GetComponentsInChildren<Warp>();
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        class LocalRotaionConstraintJOb : IJob
        {
            public void Execute()
            {

            }
        }

        JobHandle Schedule(
            in FrameInfo info,
            TransformAccessArray transformAccessArray
        )
        {
            JobHandle handle = default;

            // load transform

            // verlet
            handle = new VerletJob
            {
            }.Schedule(_verletCurrent.Length, 128, handle);

            // local rotation constraint
            handle = new LocalRotationConstraintJob
            {
            }.Schedule(_warps.Length, 1, handle);

            // update verlet

            // writeback transform

            return handle;
        }

        void IRotateParticleSystem.Initialize(IEnumerable<Warp> warps, IEnumerable<RectCloth> cloths)
        {
            throw new NotImplementedException();
        }

        void IRotateParticleSystem.Process(float deltaTime)
        {
            Schedule(
                new FrameInfo(deltaTime, Vector3.zero),
                _transformAccessArray
            ).Complete();
        }

        void IRotateParticleSystem.ResetInitialRotation()
        {
            throw new NotImplementedException();
        }

        void IRotateParticleSystem.DrawGizmos()
        {
            throw new NotImplementedException();
        }
    }
}