using System;
using Unity.Collections;
using Unity.Jobs;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Mathematics;
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
        [ReadOnly] public NativeArray<float3> PrevTail;
        [ReadOnly] public NativeArray<float3> CurrentTail;
        // 処理後の tail 位置(ランダムアクセス)
        [NativeDisableParallelForRestriction] public NativeArray<float3> NextTail;
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
                    var newPosition = MultiplyPoint3x4(parentTransform.Value.localToWorldMatrix, headTransform.localPosition);
                    var newRotation = math.mul(parentTransform.Value.rotation, headTransform.localRotation);

                    headTransform = new BlittableTransform(
                        position: newPosition,
                        rotation: newRotation,
                        localPosition: headTransform.localPosition,
                        localRotation: headTransform.localRotation,
                        localScale: headTransform.localScale,
                        localToWorldMatrix: headTransform.localToWorldMatrix,
                        worldToLocalMatrix: headTransform.worldToLocalMatrix);

                    headTransform = headTransform.SetPosition(MultiplyPoint3x4(parentTransform.Value.localToWorldMatrix, headTransform.localPosition));
                    headTransform = headTransform.SetRotation(math.mul(parentTransform.Value.rotation, headTransform.localRotation));
                }

                var currentTail = centerTransform.HasValue
                    ? MultiplyPoint3x4(centerTransform.Value.localToWorldMatrix, CurrentTail[logicIndex])
                    : CurrentTail[logicIndex];
                var prevTail = centerTransform.HasValue
                    ? MultiplyPoint3x4(centerTransform.Value.localToWorldMatrix, PrevTail[logicIndex])
                    : PrevTail[logicIndex];

                var parentRotation = parentTransform?.rotation ?? quaternion.identity;

                // scaling 対応
                var scalingFactor = model.SupportsScalingAtRuntime ? TransformExtensions.AbsoluteMaxValue(headTransform.lossyScale) : 1.0f;

                // verlet積分で次の位置を計算
                var external = (joint.gravityDir * joint.gravityPower + model.ExternalForce) * DeltaTime;
                var nextTail = currentTail
                               + (currentTail - prevTail) * (1.0f - joint.dragForce) // 前フレームの移動を継続する(減衰もあるよ)
                               + math.mul(math.mul(parentRotation, logic.localRotation), logic.boneAxis) *
                               joint.stiffnessForce * DeltaTime * scalingFactor // 親の回転による子ボーンの移動目標
                               + external * scalingFactor; // 外力による移動量

                // 長さをboneLengthに強制
                nextTail = headTransform.position + math.normalize(nextTail - headTransform.position) * logic.length;

                // Collisionで移動
                for (var colliderIndex = colliderSpan.startIndex; colliderIndex < colliderSpan.startIndex + colliderSpan.count; ++colliderIndex)
                {
                    var collider = Colliders[colliderIndex];
                    var colliderTransform = Transforms[collider.transformIndex + transformIndexOffset];
                    var colliderScale = colliderTransform.lossyScale;
                    var maxColliderScale = math.max(math.max(math.abs(colliderScale.x), math.abs(colliderScale.y)), math.abs(colliderScale.z));
                    var worldPosition = MultiplyPoint3x4(colliderTransform.localToWorldMatrix, collider.offset);
                    var worldTail = MultiplyPoint3x4(colliderTransform.localToWorldMatrix, collider.tailOrNormal);

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
                    ? MultiplyPoint3x4(centerTransform.Value.worldToLocalMatrix, nextTail)
                    : nextTail;

                //回転を適用
                var rotation = math.mul(parentRotation, logic.localRotation);
                headTransform = headTransform.SetRotation(UnityEngine.Quaternion.FromToRotation(math.mul(rotation, logic.boneAxis),
                                                                                    nextTail - headTransform.position) * rotation);

                // Transformを更新
                if (parentTransform.HasValue)
                {
                    var parentLocalToWorldMatrix = parentTransform.Value.localToWorldMatrix;
                    headTransform = headTransform.SetLocalRotation(math.normalize(math.mul(math.inverse(parentTransform.Value.rotation), headTransform.rotation)));
                    headTransform = headTransform.SetLocalToWorldMatrix(math.mul(
                                                                            parentLocalToWorldMatrix,
                                                                            float4x4.TRS(
                                                                                headTransform.localPosition,
                                                                                headTransform.localRotation,
                                                                                headTransform.localScale
                                                                                )));
                }
                else
                {
                    headTransform = headTransform.SetLocalToWorldMatrix(float4x4.TRS(
                                                                            headTransform.position,
                                                                            headTransform.rotation,
                                                                            headTransform.localScale
                                                                            ));
                    headTransform = headTransform.SetLocalRotation(headTransform.rotation);
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

        private static void ResolveCapsuleCollision(
            float3 worldTail,
            float3 worldPosition,
            BlittableTransform headTransform,
            BlittableJointMutable joint,
            BlittableCollider collider,
            float maxColliderScale,
            BlittableJointImmutable logic,
            ref float3 nextTail)
        {
            var direction = worldTail - worldPosition;
            if (math.lengthsq(direction) == 0)
            {
                // head側半球の球判定
                ResolveSphereCollision(joint, collider, worldPosition, headTransform, maxColliderScale, logic, ref nextTail);
                return;
            }
            var P = math.normalize(direction);
            var Q = headTransform.position - worldPosition;
            var dot = math.dot(P, Q);
            if (dot <= 0)
            {
                // head側半球の球判定
                ResolveSphereCollision(joint, collider, worldPosition, headTransform, maxColliderScale, logic, ref nextTail);
                return;
            }
            if (dot >= math.length(direction))
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
            float3 worldPosition,
            BlittableTransform headTransform,
            float maxColliderScale,
            BlittableJointImmutable logic,
            ref float3 nextTail)
        {
            var r = joint.radius + collider.radius * maxColliderScale;
            if (math.lengthsq(nextTail - worldPosition) <= (r * r))
            {
                // ヒット。Colliderの半径方向に押し出す
                var normal = math.normalize(nextTail - worldPosition);
                var posFromCollider = worldPosition + normal * r;
                // 長さをboneLengthに強制
                nextTail = headTransform.position + math.normalize(posFromCollider - headTransform.position) * logic.length;
            }
        }

        private static void ResolveSphereCollisionInside(
            BlittableJointMutable joint,
            BlittableCollider collider,
            BlittableTransform colliderTransform,
            ref float3 nextTail)
        {
            var transformedOffset = MultiplyPoint(colliderTransform.localToWorldMatrix, collider.offset);
            var delta = nextTail - transformedOffset;

            // ジョイントとコライダーの距離。負の値は衝突していることを示す
            var distance = collider.radius - joint.radius - math.length(delta);

            // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
            if (distance < 0)
            {
                var direction = -1 * math.normalize(delta);
                nextTail -= direction * distance;
            }
        }

        private static void ResolveCapsuleCollisionInside(
            BlittableJointMutable joint,
            BlittableCollider collider,
            BlittableTransform colliderTransform,
            ref float3 nextTail)
        {
            var transformedOffset = MultiplyPoint(colliderTransform.localToWorldMatrix, collider.offset);
            var transformedTail = MultiplyPoint(colliderTransform.localToWorldMatrix, collider.tailOrNormal);
            var offsetToTail = transformedTail - transformedOffset;
            var lengthSqCapsule = math.lengthsq(offsetToTail);

            var delta = nextTail - transformedOffset;
            var dot = math.dot(offsetToTail, delta);

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
            var distance = collider.radius - joint.radius - math.length(delta);

            // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
            if (distance < 0)
            {
                var direction = -1 * math.normalize(delta);
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
            ref float3 nextTail)
        {
            var transformedOffset = MultiplyPoint(colliderTransform.localToWorldMatrix, collider.offset);
            var transformedNormal = math.normalize(MultiplyVector(colliderTransform.localToWorldMatrix, collider.tailOrNormal));
            var delta = nextTail - transformedOffset;

            // ジョイントとコライダーの距離。負の値は衝突していることを示す
            var distance = math.dot(delta, transformedNormal) - joint.radius;

            if (distance < 0)
            {
                // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
                var direction = transformedNormal;
                nextTail -= direction * distance;
            }
        }
        
        private static float3 MultiplyPoint3x4(float4x4 matrix, float3 point)
        {
            return math.mul(matrix, new float4(point, 1)).xyz;
        }
        
        private static float3 MultiplyPoint(float4x4 matrix, float3 point)
        {
            var v = math.mul(matrix, new float4(point, 1.0f));
            return v.xyz / v.w;
        }

        private static float3 MultiplyVector(float4x4 matrix, float3 vector)
        {
            return math.mul(matrix, new float4(vector, 0)).xyz;
        }
    }
}