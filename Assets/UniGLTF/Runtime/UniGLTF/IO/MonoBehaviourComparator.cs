using System;
using UnityEngine;


namespace UniGLTF
{
    public static class MonoBehaviourComparator
    {
        public static bool AssertAreEquals(GameObject l, GameObject r)
        {
            return
                l.name == r.name
                && AssertAreEquals<Transform>(l, r,
                (x, y) => AssertAreEquals(x[0], y[0]))
                && AssertAreEquals<MeshFilter>(l, r,
                (x, y) => AssertAreEquals(x[0], y[0]))
                && AssertAreEquals<MeshRenderer>(l, r,
                (x, y) => AssertAreEquals(x[0], y[0]))
                && AssertAreEquals<SkinnedMeshRenderer>(l, r,
                (x, y) => AssertAreEquals(x[0], y[0]))
                ;
        }

        public static bool AssertAreEquals<T>(GameObject l, GameObject r, Func<T[], T[], bool> pred) where T : Component
        {
            var ll = l.GetComponents<T>();
            var rr = r.GetComponents<T>();
            if (ll.Length != rr.Length)
            {
                return false;
            }
            if (ll.Length == 0)
            {
                return true;
            }
            return pred(ll, rr);
        }

        public static bool AssertAreEquals(Transform l, Transform r)
        {
            return
            (l.localPosition == r.localPosition)
            && (l.localRotation == r.localRotation)
            && (l.localScale == r.localScale)
                ;
        }

        public static bool AssertAreEquals(MeshFilter l, MeshFilter r)
        {
            throw new NotImplementedException();
        }

        public static bool AssertAreEquals(MeshRenderer l, MeshRenderer r)
        {
            throw new NotImplementedException();
        }

        public static bool AssertAreEquals(SkinnedMeshRenderer l, SkinnedMeshRenderer r)
        {
            throw new NotImplementedException();
        }

        public static bool AssetAreEquals(Texture2D l, Texture2D r)
        {
            throw new NotImplementedException();
        }
    }
}
