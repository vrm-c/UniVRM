using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;


namespace UniVRM10.ClothWarp.Jobs
{
    public struct BranchInfo
    {
        public int FirstSiblingIndex;
    }

    public struct TransformInfo
    {
        public TransformType TransformType;
        public int ParentIndex;
        public Quaternion InitLocalRotation;
        public Vector3 InitLocalPosition;
        public ParticleSettings Settings;
        public int WarpIndex;
        public BranchInfo? Branch;
    }

    public struct TransformData
    {
        public Matrix4x4 ToWorld;
        public Vector3 Position => ToWorld.GetPosition();
        public Quaternion Rotation => ToWorld.rotation;
        public Matrix4x4 ToLocal;

        public TransformData(TransformAccess t)
        {
            ToWorld = t.localToWorldMatrix;
            ToLocal = t.worldToLocalMatrix;
        }
        public TransformData(Transform t)
        {
            ToWorld = t.localToWorldMatrix;
            ToLocal = t.worldToLocalMatrix;
        }
    }

    // [Input]
    public struct InputTransformJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [WriteOnly] public NativeArray<TransformData> InputData;
        [WriteOnly] public NativeArray<Vector3> CurrentPositions;

        [WriteOnly] public NativeArray<int> CollisionCount;
        [WriteOnly] public NativeArray<Vector3> CollisionDelta;
        [WriteOnly] public NativeArray<Vector3> ImpulsiveForces;

        public void Execute(int particleIndex, TransformAccess transform)
        {
            InputData[particleIndex] = new TransformData(transform);

            var particle = Info[particleIndex];
            if (particle.TransformType.PositionInput())
            {
                // only warp root position update
                CurrentPositions[particleIndex] = transform.position;
            }

            // clear cloth
            CollisionCount[particleIndex] = 0;
            CollisionDelta[particleIndex] = Vector3.zero;
            ImpulsiveForces[particleIndex] = Vector3.zero;
        }
    }

    // [Output]
    public struct OutputTransformJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<Quaternion> NextRotations;
        public void Execute(int particleIndex, TransformAccess transform)
        {
            var info = Info[particleIndex];
            if (info.TransformType.Writable())
            {
                transform.rotation = NextRotations[particleIndex];
            }
        }
    }
}