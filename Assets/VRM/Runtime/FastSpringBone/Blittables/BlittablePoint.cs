using UnityEngine;

namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// VRMSpringBoneLogicをBlittableにしたもの
    /// ベルレ積分など、コアな計算を行う
    /// </summary>
    public unsafe struct BlittablePoint
    {
        private readonly float _length;
        private readonly Quaternion _localRotation;
        private readonly Vector3 _boneAxis;
        private readonly float _radius;
        private Vector3 _prevPosition;

        private readonly BlittableColliderGroups* _blittableColliderGroups;
        private readonly BlittableTransform* _center;

        public Vector3 CurrentPosition { get; private set; }

        private readonly BlittableTransform* _transform;

        public BlittablePoint(
            Transform transform,
            float radius,
            BlittableTransform* center,
            BlittableColliderGroups* blittableColliderGroups,
            BlittableTransform* blittableTransform)
        {
            Vector3 localPosition;
            if (transform.childCount == 0)
            {
                var delta = transform.position - transform.parent.position;
                var childPosition = transform.position + delta.normalized * 0.07f;
                localPosition = transform.worldToLocalMatrix.MultiplyPoint(childPosition);
            }
            else
            {
                var firstChild = transform.GetChild(0);
                var scale = firstChild.lossyScale;
                localPosition = firstChild.localPosition;
                localPosition.x *= scale.x;
                localPosition.y *= scale.y;
                localPosition.z *= scale.z;
            }

            var worldChildPosition = (Vector3)transform.TransformPoint(localPosition);
            _prevPosition = CurrentPosition = center != null ? center->LocalToWorld.inverse.MultiplyPoint3x4(worldChildPosition) : worldChildPosition;

            _localRotation = transform.localRotation;
            _boneAxis = localPosition.normalized;
            _length = localPosition.magnitude;
            _radius = radius;
            _blittableColliderGroups = blittableColliderGroups;
            _transform = blittableTransform;
            _center = center;
        }

        public void Update(float stiffnessForce, float dragForce, Vector3 external)
        {
            // 親のRotationが変わっている可能性があるので更新する
            _transform->UpdateLocalToWorldMatrix();

            Vector3 currentPosition;
            Vector3 prevPosition;

            if (_center == null)
            {
                currentPosition = CurrentPosition;
                prevPosition = _prevPosition;
            }
            else
            {
                var centerLocalToWorld = _center->LocalToWorld;
                currentPosition = centerLocalToWorld.MultiplyPoint3x4(CurrentPosition);
                prevPosition = centerLocalToWorld.MultiplyPoint3x4(_prevPosition);
            }

            // verlet積分で次の位置を計算
            var nextPosition = currentPosition
                + (currentPosition - prevPosition) * (1.0f - dragForce) // 前フレームの移動を継続する(減衰もあるよ)
                + _transform->ParentWorldRotation * _localRotation * _boneAxis * stiffnessForce // 親の回転による子ボーンの移動目標
                + external; // 外力による移動量

            // 長さをboneLengthに強制
            var position = _transform->WorldPosition;
            nextPosition = position + (nextPosition - position).normalized * _length;

            nextPosition = Collision(nextPosition, position);

            if (_center == null)
            {
                _prevPosition = currentPosition;
                CurrentPosition = nextPosition;
            }
            else
            {
                var centerWorldToLocal = _center->LocalToWorld.inverse;
                _prevPosition = centerWorldToLocal.MultiplyPoint3x4(currentPosition);
                CurrentPosition = centerWorldToLocal.MultiplyPoint3x4(nextPosition);
            }

            //回転を適用
            _transform->SetWorldRotation(ApplyRotation(nextPosition));
        }

        private Vector3 Collision(Vector3 nextPosition, Vector3 position)
        {
            for (var i = 0; i < _blittableColliderGroups->Length; ++i)
            {
                var colliderGroup = (*_blittableColliderGroups)[i];

                for (var j = 0; j < colliderGroup.Colliders.Count; ++j)
                {
                    var collider = colliderGroup.Colliders[j];
                    var colliderPosition = colliderGroup.Transform->TransformPoint(collider.Offset);
                    var r = _radius + collider.Radius;
                    
                    if (!((nextPosition - colliderPosition).sqrMagnitude <= (r * r))) continue;
                    
                    // ヒット。Colliderの半径方向に押し出す
                    var normal = (nextPosition - colliderPosition).normalized;
                    var posFromCollider = colliderPosition + normal * (_radius + collider.Radius);

                    // 長さをboneLengthに強制
                    nextPosition = position + (posFromCollider - position).normalized * _length;
                }
            }
            return nextPosition;
        }

        private static Quaternion FromToRotation(Vector3 from, Vector3 to)
            => Quaternion.AxisAngle(
                angle: Mathf.Acos(Mathf.Clamp(Vector3.Dot(from.normalized, to.normalized), -1f, 1f)),
                axis: Vector3.Cross(from, to).normalized
            );

        private Quaternion ApplyRotation(Vector3 nextTail)
        {
            var rotation = _transform->ParentWorldRotation * _localRotation;
            return 
                FromToRotation(
                    rotation * _boneAxis,
                    nextTail - _transform->WorldPosition) *
                rotation;
        }
    }
}
