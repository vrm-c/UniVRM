using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;


namespace RotateParticle.Jobs
{
    public struct TransformInfo
    {
        public TransformType TransformType;
        public int ParentIndex;
        public Quaternion InitLocalRotation;
        public Vector3 InitLocalPosition;
        public BlittableJointMutable Settings;
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
        public void Execute(int particleIndex, TransformAccess transform)
        {
            InputData[particleIndex] = new TransformData(transform);

            var particle = Info[particleIndex];
            if (particle.TransformType.PositionInput())
            {
                // only warp root position update
                CurrentPositions[particleIndex] = transform.position;
            }
        }
    }

    public struct VerletJob : IJobParallelFor
    {
        public FrameInfo Frame;
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<TransformData> CurrentTransforms;
        [ReadOnly] public NativeArray<Vector3> CurrentPositions;
        [ReadOnly] public NativeArray<Vector3> PrevPositions;
        [WriteOnly] public NativeArray<Vector3> NextPositions;
        [WriteOnly] public NativeArray<Quaternion> NextRotations;

        public void Execute(int particleIndex)
        {
            var particle = Info[particleIndex];
            if (particle.TransformType.Movable())
            {
                var parentIndex = particle.ParentIndex;
                var parentPosition = CurrentPositions[parentIndex];
                var parent = Info[parentIndex];
                var parentParentRotation = CurrentTransforms[parent.ParentIndex].Rotation;

                var external = (particle.Settings.gravityDir * particle.Settings.gravityPower + Frame.Force) * Frame.DeltaTime;

                var newPosition = CurrentPositions[particleIndex]
                     + (CurrentPositions[particleIndex] - PrevPositions[particleIndex]) * (1.0f - particle.Settings.dragForce)
                     + parentParentRotation * parent.InitLocalRotation * particle.InitLocalPosition *
                           particle.Settings.stiffnessForce * Frame.DeltaTime // 親の回転による子ボーンの移動目標
                     + external
                     ;

                NextPositions[particleIndex] = newPosition;
            }
            else
            {
                // kinematic
                NextPositions[particleIndex] = CurrentPositions[particleIndex];
            }

            NextRotations[particleIndex] = CurrentTransforms[particleIndex].Rotation;
        }
    }

    /// <summary>
    /// 親の位置に依存。再帰
    /// </summary>
    public struct ParentLengthConstraintJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<WarpInfo> Warps;
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> NextPositions;
        public void Execute(int warpIndex)
        {
            var warp = Warps[warpIndex];
            for (int particleIndex = warp.StartIndex; particleIndex < warp.EndIndex - 1; ++particleIndex)
            {
                // 位置を長さで拘束
                NextPositions[particleIndex + 1] = NextPositions[particleIndex] +
                    (NextPositions[particleIndex + 1] - NextPositions[particleIndex]).normalized
                    * Info[particleIndex + 1].InitLocalPosition.magnitude;
            }
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
            for (int particleIndex = warp.StartIndex; particleIndex < warp.EndIndex - 1; ++particleIndex)
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