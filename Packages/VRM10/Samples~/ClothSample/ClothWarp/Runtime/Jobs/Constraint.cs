using SphereTriangle;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace UniVRM10.ClothWarp.Jobs
{
    /// <summary>
    /// 親の位置に依存。再帰
    /// </summary>
    public struct ParentLengthConstraintJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<WarpInfo> Warps;
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<TransformData> Data;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> NextPositions;
        public void Execute(int warpIndex)
        {
            var warp = Warps[warpIndex];
            for (int particleIndex = warp.PrticleRange.Start; particleIndex < warp.PrticleRange.End - 1; ++particleIndex)
            {
                var particle = Info[particleIndex + 1];
                if (particle.Branch.HasValue)
                {
                    var branch = particle.Branch.Value;
                    // １番目の兄弟の情報を使う
                    // 枝分かれ。特別処理
                    var firstSibling = Info[branch.FirstSiblingIndex];
                    var firstPosition = NextPositions[branch.FirstSiblingIndex];
                    var local_d = particle.InitLocalPosition - firstSibling.InitLocalPosition;
                    NextPositions[particleIndex + 1] = firstPosition + Data[particle.ParentIndex].Rotation * local_d;
                }
                else
                {
                    // 位置を長さで拘束
                    NextPositions[particleIndex + 1] = NextPositions[particleIndex] +
                        (NextPositions[particleIndex + 1] - NextPositions[particleIndex]).normalized
                        * Info[particleIndex + 1].InitLocalPosition.magnitude;
                }
            }
        }
    }

    public struct WeftConstraintJob : IJobParallelFor
    {
        public float Hookean;
        [ReadOnly] public NativeArray<(int, SpringConstraint, ClothRect)> ClothRects;
        [ReadOnly] public NativeArray<Vector3> CurrentPositions;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> ImpulsiveForces;

        public void Execute(int rectIndex)
        {
            var (clothGridIndex, spring, rect) = ClothRects[rectIndex];
            var p0 = CurrentPositions[spring._p0];
            var p1 = CurrentPositions[spring._p1];
            var d = Vector3.Distance(p0, p1);
            var f = (d - spring._rest) * Hookean;
            var dx = (p1 - p0).normalized * f;
            ImpulsiveForces[spring._p0] += dx;
            ImpulsiveForces[spring._p1] -= dx;
        }
    }

    /// <summary>
    /// 親の回転に依存。再帰
    /// </summary>
    public struct ApplyRotationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<WarpInfo> Warps;
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<TransformData> CurrentTransforms;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> NextPositions;
        [NativeDisableParallelForRestriction] public NativeArray<Quaternion> NextRotations;
        public void Execute(int warpIndex)
        {
            var warp = Warps[warpIndex];
            for (int particleIndex = warp.PrticleRange.Start; particleIndex < warp.PrticleRange.End - 1; ++particleIndex)
            {
                //回転を適用
                var p = Info[particleIndex];
                var rotation = NextRotations[p.ParentIndex] * Info[particleIndex].InitLocalRotation;
                NextRotations[particleIndex] = Quaternion.FromToRotation(
                    rotation * Info[particleIndex + 1].InitLocalPosition,
                    NextPositions[particleIndex + 1] - NextPositions[particleIndex]) * rotation;
            }
        }
    }
}