using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


namespace RotateParticle.Jobs
{
    public enum ParticleType
    {
        // kinematic. not verlet move
        Root,
        // particle. verlet move
        Verlet,
        // branch. move same as 1st branch sibling
        Branch,
    }


    /// <summary>
    /// Reconstruct されるまで不変
    /// </summary>
    public struct ParticleImmutable
    {
        public ParticleType ParticleType;
        public int WarpIndex;
        public int ParentIndex;
        public float DistanceFromParent;
        public Quaternion InitRotation;
        public Vector3 InitTailLocalPosition;
    }

    /// <summary>
    /// 毎フレーム変化するかもしれない
    /// </summary>
    public struct ParticleMutable
    {
        public float DragForce;
    }

    /// <summary>
    /// Reconstruct されるまで不変
    /// </summary>
    public struct WarpImmutable
    {
        public int ParentIndex;
        public int BranchRootIndex;
        public int Start;
        public int End;
    }

    /// <summary>
    /// 毎フレーム変化するかもしれない
    /// </summary>
    public struct WarpMutable
    {
        /// <summary>
        /// center が未指定ならば Vector3.zero
        /// </summary>
        public Vector3 Center;
    }

    public struct ReadTransformJob : IJobParallelForTransform
    {
        [WriteOnly] public NativeArray<Vector3> CurrentPositions;
        [WriteOnly] public NativeArray<Quaternion> NextRotations;
        public void Execute(int index, TransformAccess transform)
        {
            // TODO: ModelSpace
            CurrentPositions[index] = transform.position;
            NextRotations[index] = transform.rotation;
        }
    }

    public struct WritebackTransformJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<Vector3> NextPositions;
        [ReadOnly] public NativeArray<Quaternion> NextRotations;
        public void Execute(int index, TransformAccess transform)
        {
            // TODO: ModelSpace
            transform.position = NextPositions[index];
            transform.rotation = NextRotations[index];
        }
    }

    struct VerletJob : IJobParallelFor
    {
        public bool ZeroVelocity;
        [ReadOnly] public FrameInfo Frame;

        // [ReadOnly] public NativeArray<WarpImmutable> WarpImmutables;
        [ReadOnly] public NativeArray<WarpMutable> WarpMutables;

        [ReadOnly] public NativeArray<ParticleImmutable> ParticleImmutables;
        [ReadOnly] public NativeArray<ParticleMutable> ParticleMutables;
        [ReadOnly] public NativeArray<Vector3> PrevPositionsOnCenter;
        [ReadOnly] public NativeArray<Vector3> CurrentPositions;
        [WriteOnly] public NativeArray<Vector3> NextPositions;
        public void Execute(int index)
        {
            var particle = ParticleImmutables[index];
            var particleMutable = ParticleMutables[index];
            if (particle.ParticleType == ParticleType.Root)
            {
                // kinematic. not move
                NextPositions[index] = CurrentPositions[index];
            }
            else
            {
                var warp = WarpMutables[particle.WarpIndex];
                var velocity = ZeroVelocity
                    ? Vector3.zero
                    : CurrentPositions[index] - warp.Center - PrevPositionsOnCenter[index]
                    ;
                velocity *= 1 - particleMutable.DragForce;
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
        [ReadOnly] public NativeArray<WarpImmutable> WarpImmutables;
        [ReadOnly] public NativeArray<ParticleImmutable> ParticleImmutables;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> Positions;
        [NativeDisableParallelForRestriction] public NativeArray<Quaternion> Rotations;

        public void Execute(int warpIndex)
        {
            var warp = WarpImmutables[warpIndex];
            for (int i = warp.Start; i < warp.End; ++i)
            {
                var particle = ParticleImmutables[i];
                switch (particle.ParticleType)
                {
                    case ParticleType.Root:
                        break;

                    case ParticleType.Verlet:
                        if (i + 1 < warp.End)
                        {
                            var next = ParticleImmutables[i + 1];
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

    struct VerletStatusJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<WarpImmutable> WarpImmutables;
        [ReadOnly] public NativeArray<WarpMutable> WarpMutables;
        [ReadOnly] public NativeArray<Vector3> CurrentPositions;
        [WriteOnly] public NativeArray<Vector3> PrevPositionsOnCenter;

        public void Execute(int warpIndex)
        {
            var warp = WarpImmutables[warpIndex];
            var warpMutable = WarpMutables[warpIndex];
            for (int i = warp.Start; i < warp.End; ++i)
            {
                PrevPositionsOnCenter[i] = CurrentPositions[i] - warpMutable.Center;
            }
        }
    }
}