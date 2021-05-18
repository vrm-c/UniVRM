using UnityEngine;

namespace UniVRM10
{
    public struct TR
    {
        public Quaternion Rotation;
        public Vector3 Translation;

        public static TR Identity => new TR(Quaternion.identity, Vector3.zero);

        public static TR FromWorld(Transform t) => new TR(t.rotation, t.position);

        public static TR FromParent(Transform t) => t.parent != null ? FromWorld(t.parent) : TR.Identity;

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


        /// <summary>
        /// R1|T1 R2|T2 x    R1R2|R1T2+T1 x
        /// --+-- --+-- y => ----+------- y
        ///  0| 1  0| 1 z       0|      1 z
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static TR operator *(TR a, TR b) => new TR(a.Rotation * b.Rotation, a.Rotation * b.Translation + a.Translation);

        /// <summary>
        /// R|0 1|T    R|RT
        /// -+- -+- => -+--
        /// 0|1 0|1    0| 1
        /// </summary>
        public TR Inverse()
        {
            var inv = Quaternion.Inverse(Rotation);
            return new TR(inv, inv * -Translation);
        }
    }
}
