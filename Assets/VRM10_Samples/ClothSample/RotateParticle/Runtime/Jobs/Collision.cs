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

        public void Execute(int index, TransformAccess transform)
        {
            CurrentCollider[index] = transform.localToWorldMatrix;
        }
    }

    public struct CollisionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<BlittableCollider> Colliders;
        [ReadOnly] public NativeArray<Matrix4x4> CurrentColliders;
        [WriteOnly] public NativeArray<Vector3> NextPositions;

        public void Execute(int index)
        {
            for (int i = 0; i < Colliders.Length; ++i)
            {
                var c = Colliders[i];
                switch (c.colliderType)
                {
                    case BlittableColliderType.Sphere:
                        break;

                    default:
                        // TODO
                        break;
                }
            }
            // var d = Vector3.Distance(from, to);
            // if (d > (ra + rb))
            // {
            //     resolved = default;
            //     return false;
            // }
            // Vector3 normal = (to - from).normalized;
            // resolved = new(from, from + normal * (d - rb));
            // return true;
        }
    }
}