using UnityEngine;

namespace UniGLTF.Utils
{
    public readonly struct TransformState
    {
        public Vector3 Position { get; }
        public Vector3 LocalPosition { get; }
        public Quaternion Rotation { get; }
        public Quaternion LocalRotation { get; }
        public Vector3 LocalScale { get; }

        public TransformState(Vector3 position, Vector3 localPosition, Quaternion rotation, Quaternion localRotation, Vector3 localScale)
        {
            Position = position;
            LocalPosition = localPosition;
            Rotation = rotation;
            LocalRotation = localRotation;
            LocalScale = localScale;
        }

        public TransformState(Transform tf)
        {
            if (tf == null)
            {
                Position = Vector3.zero;
                LocalPosition = Vector3.zero;
                Rotation = Quaternion.identity;
                LocalRotation = Quaternion.identity;
                LocalScale = Vector3.one;
            }
            else
            {
                Position = tf.position;
                LocalPosition = tf.localPosition;
                Rotation = tf.rotation;
                LocalRotation = tf.localRotation;
                LocalScale = tf.localScale;
            }
        }
    }
}