using SphereTriangle;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace UniVRM10.ClothWarp.Jobs
{
    public struct WarpCollisionJob : IJobParallelFor
    {
        // collider
        [ReadOnly] public NativeArray<BlittableCollider> Colliders;
        [ReadOnly] public NativeArray<Matrix4x4> CurrentColliders;

        // particle
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<Vector3> NextPositions;
        [ReadOnly] public NativeArray<bool> ClothUsedParticles;
        [WriteOnly] public NativeArray<Vector3> StrandCollision;

        // collider group
        [ReadOnly] public NativeArray<WarpInfo> Warps;
        [ReadOnly] public NativeArray<int> ColliderGroupRef;
        [ReadOnly] public NativeArray<ArrayRange> ColliderGroup;
        [ReadOnly] public NativeArray<int> ColliderRef;

        public void Execute(int particleIndex)
        {
            if (
                // cloth でない
                !ClothUsedParticles[particleIndex]
                // 枝のjointでない
                && !Info[particleIndex].Branch.HasValue
            )
            {
                var info = Info[particleIndex];
                var pos = NextPositions[particleIndex];
                var warp = Warps[info.WarpIndex];

                for (int groupRefIndex = warp.ColliderGroupRefRange.Start; groupRefIndex < warp.ColliderGroupRefRange.End; ++groupRefIndex)
                {
                    var groupIndex = ColliderGroupRef[groupRefIndex];
                    var group = ColliderGroup[groupIndex];
                    for (int colliderRefIndex = group.Start; colliderRefIndex < group.End; ++colliderRefIndex)
                    {
                        var colliderIndex = ColliderRef[colliderRefIndex];
                        var c = Colliders[colliderIndex];
                        var m = CurrentColliders[c.transformIndex];

                        if (c.colliderType == BlittableColliderType.Capsule)
                        {
                            if (SphereSphereCollision.TryCollideCapsuleAndSphere(m.MultiplyPoint(c.offset), m.MultiplyPoint(c.tailOrNormal), c.radius,
                                pos, info.Settings.Radius, out var l))
                            {
                                pos += l.GetDelta(c.radius);
                            }
                        }
                        else
                        {
                            if (SphereSphereCollision.TryCollideSphereAndSphere(m.MultiplyPoint(c.offset), c.radius,
                                pos, info.Settings.Radius, out var l))
                            {
                                pos += l.GetDelta(c.radius);
                            }
                        }
                    }
                }
                StrandCollision[particleIndex] = pos;
            }
        }
    }
}