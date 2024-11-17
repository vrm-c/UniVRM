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

    public struct ParticleInfo
    {
        public ParticleType ParticleType;
        public int WarpIndex;
        public int ParentIndex;
        public float DistanceFromParent;
        public Quaternion InitRotation;
        public Vector3 InitTailLocalPosition;
    }

    public struct WarpInfo
    {
        public Vector3 Center;
        public float DragForce;
        public Quaternion ParentRotation;
        public int BranchRootIndex;
        public int Start;
        public int End;
    }

    public struct LoadTransformJob : IJobParallelForTransform
    {
        // [ReadOnly] public NativeArray<ParticleInfo> Particles;
        [WriteOnly] public NativeArray<Vector3> CurrentPositions;
        [WriteOnly] public NativeArray<Quaternion> CurrentRotations;
        public void Execute(int index, TransformAccess transform)
        {
            // TODO: ModelSpace
            CurrentPositions[index] = transform.position;
            CurrentRotations[index] = transform.rotation;
        }
    }

    struct VerletJob : IJobParallelFor
    {
        [ReadOnly] public FrameInfo Frame;

        [ReadOnly] public NativeArray<WarpInfo> Warps;


        [ReadOnly] public NativeArray<ParticleInfo> Particles;
        [ReadOnly] public NativeArray<Vector3> PrevPositionsOnCenter;
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
                var velocity = CurrentPositions[index] - warp.Center - PrevPositionsOnCenter[index];
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

    class LocalRotaionConstraintJob : IJob
    {
        public void Execute()
        {

        }
    }
}