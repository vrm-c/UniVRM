using UniGLTF.Runtime.Utils;
using UniGLTF.SpringBoneJobs.Blittables;
using Unity.Mathematics;

namespace UniGLTF.SpringBoneJobs
{
    public static class SpringBoneCollision
    {
        public static bool TryCollide(
            in BlittableJointImmutable logic, in BlittableJointMutable joint, in BlittableTransform headTransform,
            in BlittableCollider collider, in BlittableTransform colliderTransform, float maxColliderScale,
            in float3 colliderWorldTail, in float3 colliderWorldPosition,
            in float3 nextTail, out float3 newNextTail)
        {
            switch (collider.colliderType)
            {
                case BlittableColliderType.Sphere:
                    return TryResolveSphereCollision(joint, collider, colliderWorldPosition, headTransform, maxColliderScale, logic, nextTail, out newNextTail);

                case BlittableColliderType.Capsule:
                    return TryResolveCapsuleCollision(colliderWorldTail, colliderWorldPosition, headTransform, joint, collider, maxColliderScale, logic, nextTail, out newNextTail);

                case BlittableColliderType.Plane:
                    return TryResolvePlaneCollision(joint, collider, colliderTransform, nextTail, out newNextTail);

                case BlittableColliderType.SphereInside:
                    return TryResolveSphereCollisionInside(joint, collider, colliderTransform, nextTail, out newNextTail);

                case BlittableColliderType.CapsuleInside:
                    return TryResolveCapsuleCollisionInside(joint, collider, colliderTransform, nextTail, out newNextTail);

                default:
                    throw new System.NotImplementedException();
            }
        }

        private static bool TryResolveSphereCollision(
            in BlittableJointMutable joint,
            in BlittableCollider collider,
            in float3 worldPosition,
            in BlittableTransform headTransform,
            in float maxColliderScale,
            in BlittableJointImmutable logic,
            in float3 nextTail, out float3 newNextTail)
        {
            var r = joint.radius + collider.radius * maxColliderScale;
            if (math.lengthsq(nextTail - worldPosition) <= (r * r))
            {
                // ヒット。Colliderの半径方向に押し出す
                var normal = math.normalize(nextTail - worldPosition);
                var posFromCollider = worldPosition + normal * r;
                // 長さをboneLengthに強制
                newNextTail = headTransform.position + math.normalize(posFromCollider - headTransform.position) * logic.length;
                return true;
            }
            else
            {
                newNextTail = default;
                return false;
            }
        }

        private static bool TryResolveCapsuleCollision(
            float3 worldTail,
            float3 worldPosition,
            BlittableTransform headTransform,
            BlittableJointMutable joint,
            BlittableCollider collider,
            float maxColliderScale,
            BlittableJointImmutable logic,
            in float3 nextTail, out float3 newNextTail)
        {
            var direction = worldTail - worldPosition;
            if (math.lengthsq(direction) == 0)
            {
                // head側半球の球判定
                return TryResolveSphereCollision(joint, collider, worldPosition, headTransform, maxColliderScale, logic, nextTail, out newNextTail);
            }
            var P = math.normalize(direction);
            var Q = headTransform.position - worldPosition;
            var dot = math.dot(P, Q);
            if (dot <= 0)
            {
                // head側半球の球判定
                return TryResolveSphereCollision(joint, collider, worldPosition, headTransform, maxColliderScale, logic, nextTail, out newNextTail);
            }
            if (dot >= math.length(direction))
            {
                // tail側半球の球判定
                return TryResolveSphereCollision(joint, collider, worldTail, headTransform, maxColliderScale, logic, nextTail, out newNextTail);
            }

            // head-tail上の m_transform.position との最近点
            var p = worldPosition + P * dot;
            return TryResolveSphereCollision(joint, collider, p, headTransform, maxColliderScale, logic, nextTail, out newNextTail);
        }

        /// <summary>
        /// Collision with SpringJoint and PlaneCollider.
        /// If collide update nextTail.
        /// </summary>
        /// <param name="joint">joint</param>
        /// <param name="collider">collider</param>
        /// <param name="colliderTransform">colliderTransform.localToWorldMatrix.MultiplyPoint3x4(collider.offset);</param>
        /// <param name="nextTail">result of verlet integration</param>
        private static bool TryResolvePlaneCollision(
            in BlittableJointMutable joint,
            in BlittableCollider collider,
            in BlittableTransform colliderTransform,
            in float3 nextTail, out float3 newNextTail)
        {
            var transformedOffset = MathHelper.MultiplyPoint(colliderTransform.localToWorldMatrix, collider.offset);
            var transformedNormal = math.normalize(MathHelper.MultiplyVector(colliderTransform.localToWorldMatrix, collider.tailOrNormal));
            var delta = nextTail - transformedOffset;

            // ジョイントとコライダーの距離。負の値は衝突していることを示す
            var distance = math.dot(delta, transformedNormal) - joint.radius;

            if (distance < 0)
            {
                // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
                var direction = transformedNormal;
                newNextTail = nextTail - direction * distance;
                return true;
            }
            else
            {
                newNextTail = default;
                return false;
            }
        }

        private static bool TryResolveSphereCollisionInside(
            in BlittableJointMutable joint,
            in BlittableCollider collider,
            in BlittableTransform colliderTransform,
            in float3 nextTail, out float3 newNextTail)
        {
            var transformedOffset = MathHelper.MultiplyPoint(colliderTransform.localToWorldMatrix, collider.offset);
            var delta = nextTail - transformedOffset;

            // ジョイントとコライダーの距離。負の値は衝突していることを示す
            var distance = collider.radius - joint.radius - math.length(delta);

            // ジョイントとコライダーの距離の方向。衝突している場合、この方向にジョイントを押し出す
            if (distance < 0)
            {
                var direction = -1 * math.normalize(delta);
                newNextTail = nextTail - direction * distance;
                return true;
            }
            else
            {
                newNextTail = default;
                return false;
            }
        }

        private static bool TryResolveCapsuleCollisionInside(
            in BlittableJointMutable joint,
            in BlittableCollider collider,
            in BlittableTransform colliderTransform,
            in float3 nextTail, out float3 newNextTail)
        {
            var transformedOffset = MathHelper.MultiplyPoint(colliderTransform.localToWorldMatrix, collider.offset);
            var transformedTail = MathHelper.MultiplyPoint(colliderTransform.localToWorldMatrix, collider.tailOrNormal);
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
                newNextTail = nextTail - direction * distance;
                return true;
            }
            else
            {
                newNextTail = default;
                return false;
            }
        }
    }
}