using UnityEngine;

namespace UniVRM10
{
    public struct TR
    {
        public Quaternion Rotation;
        public Vector3 Translation;

        public static TR Identity => new TR(Quaternion.identity, Vector3.zero);

        public static TR FromWorld(Transform t) => new TR(t.rotation, t.position);

        public static TR FromLocal(Transform t) => new TR(t.localRotation, t.localPosition);

        public static TR FromRelative(Transform t, Transform from)
        {
            var toRelative = from.worldToLocalMatrix;
            return new TR(toRelative.rotation * t.rotation, toRelative.MultiplyPoint(t.position));
        }

        public TR(Quaternion r, Vector3 t)
        {
            Rotation = r;
            Translation = t;
        }

        public TR(Quaternion r) : this(r, Vector3.zero)
        {
        }

        public TR(Vector3 t) : this(Quaternion.identity, t)
        {
        }

        public Matrix4x4 TRS(float s) => Matrix4x4.TRS(Translation, Rotation, new Vector3(s, s, s));

        public static TR operator *(TR a, TR b) => new TR(a.Rotation * b.Rotation, a.Rotation * b.Translation + a.Translation);

        public TR Inverse()
        {
            var inv = Quaternion.Inverse(Rotation);
            return new TR(inv, inv * -Translation);
        }
    }
}
