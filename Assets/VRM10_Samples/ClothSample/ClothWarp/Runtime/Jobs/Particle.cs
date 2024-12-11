using System;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace UniVRM10.ClothWarp.Jobs
{
    /// <summary>
    /// UniGLTF.SpringBoneJobs.Blittables.BlittableJointMutable と同じ。
    /// Range が違う。
    /// </summary>
    [Serializable]
    public struct ParticleSettings
    {
        [Range(0, 1)]
        public float Stiffness;
        [Range(0, 1)]
        public float Deceleration;
        public Vector3 Gravity;
        public float Radius;

        public static readonly ParticleSettings Default = new ParticleSettings
        {
            Stiffness = 0.08f,
            Gravity = new Vector3(0, -1.0f, 0),
            Deceleration = 0.4f,
            Radius = 0.02f,
        };

        public void FromBlittableJointMutable(BlittableJointMutable src)
        {
            Stiffness = src.stiffnessForce;
            Gravity = src.gravityDir * src.gravityPower;
            Deceleration = src.dragForce;
            Radius = src.radius;
        }

        public BlittableJointMutable ToBlittableJointMutable()
        {
            return new BlittableJointMutable
            {
                stiffnessForce = Stiffness,
                gravityPower = 1.0f,
                gravityDir = Gravity,
                dragForce = Deceleration,
                radius = Radius,
            };
        }
    }

    public struct VerletJob : IJobParallelFor
    {
        public FrameInfo Frame;
        [ReadOnly] public NativeArray<TransformInfo> Info;
        [ReadOnly] public NativeArray<TransformData> CurrentTransforms;
        [ReadOnly] public NativeArray<Vector3> ImpulsiveForces;
        [ReadOnly] public NativeArray<Vector3> CurrentPositions;
        [ReadOnly] public NativeArray<Vector3> PrevPositions;
        [WriteOnly] public NativeArray<Vector3> NextPositions;
        [WriteOnly] public NativeArray<Quaternion> NextRotations;

        public void Execute(int particleIndex)
        {
            var particle = Info[particleIndex];
            if (particle.TransformType.Movable())
            {
                var parent = Info[particle.ParentIndex];
                var parentParentRotation = CurrentTransforms[parent.ParentIndex].Rotation;

                var local_rest = parentParentRotation * parent.InitLocalRotation * particle.InitLocalPosition;
                var world_rest = CurrentPositions[particle.ParentIndex] + local_rest;
                var resilience_force = world_rest - CurrentPositions[particleIndex];
                var velocity = (CurrentPositions[particleIndex] - PrevPositions[particleIndex]) * (1.0f - particle.Settings.Deceleration);

                var newPosition = CurrentPositions[particleIndex]
                     + velocity
                     + ImpulsiveForces[particleIndex]
                     + resilience_force * particle.Settings.Stiffness
                     + (particle.Settings.Gravity + Frame.Force) * Frame.SqDeltaTime
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
}