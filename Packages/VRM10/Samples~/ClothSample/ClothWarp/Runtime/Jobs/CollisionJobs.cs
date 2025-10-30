using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


namespace UniVRM10.ClothWarp.Jobs
{
    public struct ClothInfo
    {
        public ArrayRange ColliderGroupRefRange;
    }

    public struct InputColliderJob : IJobParallelForTransform
    {
        [WriteOnly] public NativeArray<Matrix4x4> CurrentCollider;

        public void Execute(int colliderIndex, TransformAccess transform)
        {
            CurrentCollider[colliderIndex] = transform.localToWorldMatrix;
        }
    }

    public struct CollisionApplyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> ClothUsedParticles;
        [ReadOnly] public NativeArray<Vector3> StrandCollision;

        [ReadOnly] public NativeArray<int> RectCollisionCount;
        [ReadOnly] public NativeArray<Vector3> RectCollisionDelta;
        public NativeArray<Vector3> NextPosition;

        public void Execute(int particleIndex)
        {
            if (ClothUsedParticles[particleIndex])
            {
                var count = RectCollisionCount[particleIndex];
                if (count > 0)
                {
                    // 一つの頂点が最大で近接する4つの rect で当たり判定をされる
                    // 衝突結果を足して割る
                    NextPosition[particleIndex] += RectCollisionDelta[particleIndex] / count;
                }
            }
            else
            {
                NextPosition[particleIndex] = StrandCollision[particleIndex];
            }
        }
    }
}