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
        public void Execute(int index, TransformAccess transform)
        {
            InputData[index] = new TransformData(transform);

            var particle = Info[index];
            if (particle.TransformType.PositionInput())
            {
                // only warp root position update
                CurrentPositions[index] = transform.position;
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

        public void Execute(int index)
        {
            var particle = Info[index];
            if (particle.TransformType.Movable())
            {
                var parentIndex = particle.ParentIndex;
                var parentPosition = CurrentPositions[parentIndex];
                var parent = Info[parentIndex];
                var parentParentRotation = CurrentTransforms[parent.ParentIndex].Rotation;

                var external = (particle.Settings.gravityDir * particle.Settings.gravityPower + Frame.Force) * Frame.DeltaTime;

                var newPosition = CurrentPositions[index]
                     + (CurrentPositions[index] - PrevPositions[index]) * (1.0f - particle.Settings.dragForce)
                     + parentParentRotation * parent.InitLocalRotation * particle.InitLocalPosition *
                           particle.Settings.stiffnessForce * Frame.DeltaTime // 親の回転による子ボーンの移動目標
                     + external
                     ;

                NextPositions[index] = newPosition;
            }
            else
            {
                // kinematic
                NextPositions[index] = CurrentPositions[index];
            }

            NextRotations[index] = CurrentTransforms[index].Rotation;
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
        public void Execute(int index)
        {
            var warp = Warps[index];
            for (int i = warp.StartIndex; i < warp.EndIndex - 1; ++i)
            {
                // 位置を長さで拘束
                NextPositions[i + 1] = NextPositions[i] +
                    (NextPositions[i + 1] - NextPositions[i]).normalized
                    * Info[i + 1].InitLocalPosition.magnitude;
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
        public void Execute(int index)
        {
            var warp = Warps[index];
            for (int i = warp.StartIndex; i < warp.EndIndex - 1; ++i)
            {
                //回転を適用
                var p = Info[i];
                var rotation = NextRotations[p.ParentIndex] * Info[i].InitLocalRotation;
                NextRotations[i] = Quaternion.FromToRotation(
                    rotation * Info[i + 1].InitLocalPosition,
                    NextPositions[i + 1] - NextPositions[i]) * rotation;
            }
        }
    }

    // [Output]
    public struct OutputTransformJob : IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<Quaternion> NextRotations;
        public void Execute(int index, TransformAccess transform)
        {
            var info = Info[index];
            if (info.TransformType.Writable())
            {
                transform.rotation = NextRotations[index];
            }
        }
    }
}