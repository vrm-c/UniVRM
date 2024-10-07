using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UniGLTF.SpringBoneJobs.Blittables;
#if ENABLE_SPRINGBONE_BURST
using Unity.Burst;
#endif

namespace UniGLTF.SpringBoneJobs
{
#if ENABLE_SPRINGBONE_BURST
    [BurstCompile]
#endif
    /// <summary>
    /// データの粒度
    /// - Joint Level: spring の節。Transform. stiffness など
    /// - Spring Level: spring の房。root から末端まで。この房 level で並列処理する
    /// - Model Level: 一人分。複数の房
    /// - System Level: すべての model。delta time とか
    /// </summary>
    public struct UpdateFastSpringBoneJob : IJobParallelFor
    {
        // Joint Level
        // すべての spring の joint を平坦に連結した配列
        // Joints, Logics, PrevTail, CurrentTail, NextTail は同じ index
        [ReadOnly] public NativeArray<BlittableJointMutable> Joints;
        [ReadOnly] public NativeArray<BlittableJointImmutable> Logics;
        [ReadOnly] public NativeArray<Vector3> PrevTail;
        [ReadOnly] public NativeArray<Vector3> CurrentTail;
        // 処理後の tail 位置(ランダムアクセス)
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> NextTail;
        // Spring Level
        [ReadOnly] public NativeArray<BlittableSpring> Springs;
        // Model Level
        [ReadOnly] public NativeArray<BlittableModelLevel> Models;

        [ReadOnly] public NativeArray<BlittableCollider> Colliders;
        // FastSpringBoneBuffer.Transforms を連結したもの(ランダムアクセス)
        [NativeDisableParallelForRestriction] public NativeArray<BlittableTransform> Transforms;

        // System Level
        public float DeltaTime;

        /// <param name="index">房のindex</param>
        public void Execute(int index)
        {
            var spring = Springs[index];
            var transformIndexOffset = spring.transformIndexOffset;
            var colliderSpan = spring.colliderSpan;
            var logicSpan = spring.logicSpan;
            var model = Models[spring.modelIndex];

            for (var logicIndex = logicSpan.startIndex; logicIndex < logicSpan.startIndex + logicSpan.count; ++logicIndex)
            {
                var logic = Logics[logicIndex];
                var joint = Joints[logicIndex];

                var headTransform = Transforms[logic.headTransformIndex + transformIndexOffset];
                var parentTransform = logic.parentTransformIndex >= 0
                    ? Transforms[logic.parentTransformIndex + transformIndexOffset]
                    : (BlittableTransform?)null;
                var centerTransform = spring.centerTransformIndex >= 0
                    ? Transforms[spring.centerTransformIndex + transformIndexOffset]
                    : (BlittableTransform?)null;

                // 親があったら、親に依存するTransformを再計算
                if (parentTransform.HasValue)
                {
                    headTransform.position =
                        parentTransform.Value.localToWorldMatrix.MultiplyPoint3x4(headTransform.localPosition);
                    headTransform.rotation = parentTransform.Value.rotation * headTransform.localRotation;
                }

                var currentTail = centerTransform.HasValue
                    ? centerTransform.Value.localToWorldMatrix.MultiplyPoint3x4(CurrentTail[logicIndex])
                    : CurrentTail[logicIndex];
                var prevTail = centerTransform.HasValue
                    ? centerTransform.Value.localToWorldMatrix.MultiplyPoint3x4(PrevTail[logicIndex])
                    : PrevTail[logicIndex];

                var parentRotation = parentTransform?.rotation ?? Quaternion.identity;

                // scaling 対応
                var scalingFactor = model.SupportsScalingAtRuntime ? TransformExtensions.AbsoluteMaxValue(headTransform.localToWorldMatrix.lossyScale) : 1.0f;

                // verlet積分で次の位置を計算
                var external = (joint.gravityDir * joint.gravityPower + model.ExternalForce) * DeltaTime;
                var nextTail = currentTail
                               + (currentTail - prevTail) * (1.0f - joint.dragForce) // 前フレームの移動を継続する(減衰もあるよ)
                               + parentRotation * logic.localRotation * logic.boneAxis *
                               joint.stiffnessForce * DeltaTime * scalingFactor // 親の回転による子ボーンの移動目標
                               + external * scalingFactor; // 外力による移動量

                // 長さをboneLengthに強制
                nextTail = headTransform.position + (nextTail - headTransform.position).normalized * logic.length;

                // Collisionで移動
                for (var colliderIndex = colliderSpan.startIndex; colliderIndex < colliderSpan.startIndex + colliderSpan.count; ++colliderIndex)
                {
                    var collider = Colliders[colliderIndex];
                    var colliderTransform = Transforms[collider.transformIndex + transformIndexOffset];
                    var colliderScale = colliderTransform.localToWorldMatrix.lossyScale;
                    var maxColliderScale = Mathf.Max(Mathf.Max(Mathf.Abs(colliderScale.x), Mathf.Abs(colliderScale.y)), Mathf.Abs(colliderScale.z));
                    var worldPosition = colliderTransform.localToWorldMatrix.MultiplyPoint3x4(collider.offset);
                    var worldTail = colliderTransform.localToWorldMatrix.MultiplyPoint3x4(collider.tailOrNormal);

                    switch (collider.colliderType)
                    {
                        case BlittableColliderType.Sphere:
                            ResolveSphereCollision(joint, collider, worldPosition, headTransform, maxColliderScale, logic, ref nextTail);
                            break;

                        case BlittableColliderType.Capsule:
                            ResolveCapsuleCollision(worldTail, worldPosition, headTransform, joint, collider, maxColliderScale, logic, ref nextTail);
                            break;

                        case BlittableColliderType.Plane:
                            ResolvePlaneCollision(joint, collider, colliderTransform, ref nextTail);
                            break;

                        case BlittableColliderType.SphereInside:
                            ResolveSphereCollisionInside(joint, collider, colliderTransform, ref nextTail);
                            break;

                        case BlittableColliderType.CapsuleInside:
                            ResolveCapsuleCollisionInside(joint, collider, colliderTransform, ref nextTail);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }

                NextTail[logicIndex] = centerTransform.HasValue
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
                    headTransform.localRotation = Normalize(Quaternion.Inverse(parentTransform.Value.rotation) * headTransform.rotation);
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

                if (!model.StopSpringBoneWriteback)
                {
                    // SpringBone の結果を Transform に反映する
                    Transforms[logic.headTransformIndex + transformIndexOffset] = headTransform;
                }
                else
                {
                    // SpringBone の結果を Transform に反映しないが logic の更新は継続する。
                    // 再開したときに暴れない。
                }
            }
        }

        /// <summary>
        /// BurstではMathfがエラーを吐くため、内部でMathfを呼ばないNormalizeを自前実装
        /// </summary>
        private static Quaternion Normalize(Quaternion q)
        {
            var num = (float)Math.Sqrt(Quaternion.Dot(q, q));
            return num < float.Epsilon ? Quaternion.identity : new Quaternion(q.x / num, q.y / num, q.z / num, q.w / num);
        }

        private static void ResolveCapsuleCollision(
            Vector3 worldTail,
            Vector3 worldPosition,
            BlittableTransform headTransform,
            BlittableJointMutable joint,
            BlittableCollider collider,
            float maxColliderScale,
            BlittableJointImmutable logic,
            ref Vector3 nextTail)
        {
            var direction = worldTail - worldPosition;
            if (direction.sqrMagnitude == 0)
            {
                // head側半球の球判定
                ResolveSphereCollision(joint, collider, worldPosition, headTransform, maxColliderScale, logic, ref nextTail);
                return;
            }
            var P = direction.normalized;
            var Q = headTransform.position - worldPosition;
            var dot = Vector3.Dot(P, Q);
            if (dot <= 0)
            {
                // head側半球の球判定
                ResolveSphereCollision(joint, collider, worldPosition, headTransform, maxColliderScale, logic, ref nextTail);
                return;
            }
            if (dot >= direction.magnitude)
            {
                // tail側半球の球判定
                ResolveSphereCollision(joint, collider, worldTail, headTransform, maxColliderScale, logic, ref nextTail);
                return;
            }

            // head-tail上の m_transform.position との最近点
            var p = worldPosition + P * dot;
            ResolveSphereCollision(joint, collider, p, headTransform, maxColliderScale, logic, ref nextTail);
        }

        private static void ResolveSphereCollision(
            BlittableJointMutable joint,
            BlittableCollider collider,
            Vector3 worldPosition,
            BlittableTransform headTransform,
            float maxColliderScale,
            BlittableJointImmutable logic,
            ref Vector3 nextTail)
        {
            var r = joint.radius + collider.radius * maxColliderScale;
            if (Vector3.SqrMagnitude(nextTail - worldPosition) <= (r * r))
            {
                // ヒット。Colliderの半径方向に押し出す
                var normal = (nextTail - worldPosition).normalized;
                var posFromCollider = worldPosition + normal * r;
                // 長さをboneLengthに強制
                nextTail = headTransform.position + (posFromCollider - headTransform.position).normalized * logic.length;
            }
        }

        private static void ResolveSphereCollisionInside(
            BlittableJointMutable joint,
            BlittableCollider collider,
            BlittableTransform colliderTransform,
            ref Vector3 nextTail)
        {
            var transformedOffset = colliderTransform.localToWorldMatrix.MultiplyPoint(collider.offset);
            var delta = nextTail - transformedOffset;

            // ジョイントとコライダーの距離。負の値は衝突していることを示す
            var distance = collider.radius - joint.radius - delta.magnitude;

            // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
            if (distance < 0)
            {
                var direction = -delta.normalized;
                nextTail -= direction * distance;
            }
        }

        private static void ResolveCapsuleCollisionInside(
            BlittableJointMutable joint,
            BlittableCollider collider,
            BlittableTransform colliderTransform,
            ref Vector3 nextTail)
        {
            var transformedOffset = colliderTransform.localToWorldMatrix.MultiplyPoint(collider.offset);
            var transformedTail = colliderTransform.localToWorldMatrix.MultiplyPoint(collider.tailOrNormal);
            var offsetToTail = transformedTail - transformedOffset;
            var lengthSqCapsule = offsetToTail.sqrMagnitude;

            var delta = nextTail - transformedOffset;
            var dot = Vector3.Dot(offsetToTail, delta);

            if (dot < 0.0)
            {
                // ジョイントがカプセルの始点側にある場合
                // なにもしない
            }
            else if (dot > lengthSqCapsule)
            {
                // ジョイントがカプセルの終点側にある場合
                delta -= offsetToTail;
            }
            else
            {
                // ジョイントがカプセルの始点と終点の間にある場合
                delta -= offsetToTail * (dot / lengthSqCapsule);
            }

            // ジョイントとコライダーの距離。負の値は衝突していることを示す
            var distance = collider.radius - joint.radius - delta.magnitude;

            // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
            if (distance < 0)
            {
                var direction = -delta.normalized;
                nextTail -= direction * distance;
            }
        }

        /// <summary>
        /// Collision with SpringJoint and PlaneCollider.
        /// If collide update nextTail.
        /// </summary>
        /// <param name="joint">joint</param>
        /// <param name="collider">collier</param>
        /// <param name="colliderTransform">colliderTransform.localToWorldMatrix.MultiplyPoint3x4(collider.offset);</param>
        /// <param name="nextTail">result of verlet integration</param>
        private static void ResolvePlaneCollision(
            BlittableJointMutable joint,
            BlittableCollider collider,
            BlittableTransform colliderTransform,
            ref Vector3 nextTail)
        {
            var transformedOffset = colliderTransform.localToWorldMatrix.MultiplyPoint(collider.offset);
            var transformedNormal = colliderTransform.localToWorldMatrix.MultiplyVector(collider.tailOrNormal).normalized;
            var delta = nextTail - transformedOffset;

            // ジョイントとコライダーの距離。負の値は衝突していることを示す
            var distance = Vector3.Dot(delta, transformedNormal) - joint.radius;

            if (distance < 0)
            {
                // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
                var direction = transformedNormal;
                nextTail -= direction * distance;
            }
        }
    }
}