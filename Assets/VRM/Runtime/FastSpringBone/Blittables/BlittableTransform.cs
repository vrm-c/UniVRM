using UnityEngine;
using UnityEngine.Jobs;

namespace VRM.FastSpringBones.Blittables
{
    /// <summary>
    /// Transformの必要な機能だけを絞り、Blittableに対応させたクラス
    /// </summary>
    public unsafe struct BlittableTransform
    {
        private readonly BlittableTransform* _parent;
        private Quaternion _worldRotation;
        private Vector3 _localPosition;
        private Vector3 _localScale;
        private Quaternion _localRotation;
        private Matrix4x4 _localToWorld;

        public Vector3 WorldPosition { get; private set; }

        public void SetWorldRotation(Quaternion rotation)
        {
            var parentWorldRotation = ParentWorldRotation;
            _localRotation = Quaternion.Inverse(parentWorldRotation) * rotation;
            UpdateLocalToWorldMatrix();
        }

        public Matrix4x4 LocalToWorld => _localToWorld;

        private Matrix4x4 LocalTransform => Matrix4x4.TRS(_localPosition, _localRotation, _localScale);

        public Quaternion ParentWorldRotation => _parent != null ? _parent->_worldRotation : Quaternion.identity;

        public BlittableTransform(BlittableTransform* parent, Transform transform)
        {
            _parent = parent;

            WorldPosition = transform.position;
            _worldRotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
            _localScale = transform.localScale;

            _localToWorld = transform.localToWorldMatrix;
        }

        public void PullFrom(TransformAccess transform)
        {
            WorldPosition = transform.position;
            _worldRotation = transform.rotation;
            _localPosition = transform.localPosition;
            _localRotation = transform.localRotation;
            _localScale = transform.localScale;

            _localToWorld = transform.localToWorldMatrix;
        }

        public void PushTo(TransformAccess transform)
        {
            transform.localPosition = _localPosition;
            transform.localRotation = _localRotation;
        }

        public Vector3 TransformPoint(Vector3 offset) => _localToWorld.MultiplyPoint3x4(offset);

        public void UpdateLocalToWorldMatrix()
        {
            _localToWorld = _parent == null ? LocalTransform : _parent->_localToWorld * LocalTransform;
            WorldPosition = _localToWorld.MultiplyPoint3x4(Vector3.zero);
            _worldRotation = _localToWorld.rotation;
        }
    }
}
