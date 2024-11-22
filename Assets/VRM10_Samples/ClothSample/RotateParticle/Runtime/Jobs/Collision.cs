using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


namespace RotateParticle.Jobs
{
    public struct InputColliderJob : IJobParallelForTransform
    {
        [WriteOnly] public NativeArray<Matrix4x4> CurrentCollider;

        public void Execute(int colliderIndex, TransformAccess transform)
        {
            CurrentCollider[colliderIndex] = transform.localToWorldMatrix;
        }
    }

    public struct CollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<BlittableCollider> Colliders;
        [ReadOnly] public NativeArray<Matrix4x4> CurrentColliders;
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> NextPositions;

        public void Execute(int particleIndex)
        {
            var info = Info[particleIndex];
            var pos = NextPositions[particleIndex];
            for (int colliderIndex = 0; colliderIndex < Colliders.Length; ++colliderIndex)
            {
                var c = Colliders[colliderIndex];
                switch (c.colliderType)
                {
                    case BlittableColliderType.Sphere:
                        {
                            var col_pos = CurrentColliders[colliderIndex].MultiplyPoint(c.offset);
                            var d = Vector3.Distance(pos, col_pos);
                            var min_d = info.Settings.radius + c.radius;
                            if (d < min_d)
                            {
                                Vector3 normal = (pos - col_pos).normalized;
                                pos += normal * (min_d - d);
                            }
                        }
                        break;

                    default:
                        // TODO
                        break;
                }
            }
            NextPositions[particleIndex] = pos;
        }
    }
}