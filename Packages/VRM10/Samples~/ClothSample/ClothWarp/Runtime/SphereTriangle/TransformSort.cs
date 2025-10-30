using System.Collections.Generic;
using UnityEngine;

namespace SphereTriangle
{
    public class TransformSort : IComparer<Transform>
    {
        Vector3 _o;
        public TransformSort(in Vector3 origin)
        {
            _o = origin;
        }

        static int Rot180(int d)
        {
            // d -= 180;
            while (d < 0)
            {
                d += 360;
            }
            return d;
        }

        public int Degree(Transform t)
        {
            var x = t.position.x - _o.x;
            var y = t.position.z - _o.z;
            var a = (int)(Mathf.Rad2Deg * Mathf.Atan2(x, y));
            a = Rot180(a);
            return a;
        }

        public int Compare(Transform x, Transform y)
        {
            return Degree(x) - Degree(y);
        }
    }

}