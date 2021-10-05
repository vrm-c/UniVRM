using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UniVRM10.FastSpringBones.Blittables;

namespace UniVRM10.FastSpringBones.System
{
    [BurstCompile]
    public struct UpdateFastSpringBoneJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<BlittableSpring> Springs;

        [ReadOnly] public NativeArray<BlittableJoint> Joints;
        [NativeDisableParallelForRestriction] public NativeArray<BlittableLogic> Logics;
        [NativeDisableParallelForRestriction] public NativeArray<BlittableTransform> Transforms;

        [ReadOnly] public NativeArray<BlittableCollider> Colliders;

        public float DeltaTime;

        public void Execute(int index)
        {
            var spring = Springs[index];
            var colliderSpan = spring.colliderSpan;
            var logicSpan = spring.logicSpan;

            for (var i = logicSpan.startIndex; i < logicSpan.startIndex + logicSpan.count; ++i)
            {
                var logic = Logics[i];
                var joint = Joints[i];

                var headTransform = Transforms[logic.headTransformIndex];
                var parentTransform = logic.parentTransformIndex >= 0
                    ? Transforms[logic.parentTransformIndex]
                    : (BlittableTransform?)null;
                var centerTransform = spring.centerTransformIndex >= 0
                    ? Transforms[spring.centerTransformIndex]
                    : (BlittableTransform?)null;


                // 親があったら、親に依存するTransformを再計算
                if (parentTransform.HasValue)
                {
                    headTransform.position =
                        parentTransform.Value.localToWorldMatrix.MultiplyPoint3x4(headTransform.localPosition);
                    headTransform.rotation = parentTransform.Value.rotation * headTransform.localRotation;
                }

                var currentTail = centerTransform.HasValue
                    ? centerTransform.Value.localToWorldMatrix.MultiplyPoint3x4(logic.currentTail)
                    : logic.currentTail;
                var prevTail = centerTransform.HasValue
                    ? centerTransform.Value.localToWorldMatrix.MultiplyPoint3x4(logic.prevTail)
                    : logic.prevTail;

                var parentRotation = parentTransform?.rotation ?? Quaternion.identity;

                // verlet積分で次の位置を計算
                var external = joint.gravityDir * joint.gravityPower * DeltaTime;
                var nextTail = currentTail
                               + (currentTail - prevTail) * (1.0f - joint.dragForce) // 前フレームの移動を継続する(減衰もあるよ)
                               + parentRotation * logic.localRotation * logic.boneAxis *
                               joint.stiffnessForce * DeltaTime // 親の回転による子ボーンの移動目標
                               + external; // 外力による移動量

                // 長さをboneLengthに強制
                nextTail = headTransform.position + (nextTail - headTransform.position).normalized * logic.length;

                // Collisionで移動
                //nextTail = Collision(colliders, nextTail, jointRadius);

                logic.prevTail = centerTransform.HasValue
                    ? centerTransform.Value.worldToLocalMatrix.MultiplyPoint3x4(logic.currentTail)
                    : logic.currentTail;
                logic.currentTail = centerTransform.HasValue
                    ? centerTransform.Value.worldToLocalMatrix.MultiplyPoint3x4(nextTail)
                    : nextTail;


                //回転を適用
                var rotation = parentRotation * logic.localRotation;
                headTransform.rotation = Quaternion.FromToRotation(rotation * logic.boneAxis,
                    nextTail - headTransform.position) * rotation;

                // Transformを更新
                if (parentTransform.HasValue)
                {
                    var parentLocalToWorldMatrix = parentTransform.Value.localToWorldMatrix;
                    headTransform.localRotation = Quaternion.Inverse(parentTransform.Value.rotation) * headTransform.rotation;
                    headTransform.localToWorldMatrix =
                        parentLocalToWorldMatrix *
                        Matrix4x4.TRS(
                            headTransform.localPosition,
                            headTransform.localRotation,
                            headTransform.localScale
                        );
                    headTransform.worldToLocalMatrix = headTransform.localToWorldMatrix.inverse;
                }
                else
                {
                    headTransform.localToWorldMatrix =
                        Matrix4x4.TRS(
                            headTransform.position,
                            headTransform.rotation,
                            headTransform.localScale
                        );
                    headTransform.worldToLocalMatrix = headTransform.localToWorldMatrix.inverse;
                    headTransform.localRotation = headTransform.rotation;
                }

                // 値をバッファに戻す
                Transforms[logic.headTransformIndex] = headTransform;
                Logics[i] = logic;
            }
        }
    }
}