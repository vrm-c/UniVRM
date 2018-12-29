using System;
using UnityEngine;
using VRM;


public static class VRMMonoBehaviourComparator
{
    public static bool AssertAreEquals(GameObject l, GameObject r)
    {
        return
            AssertAreEquals<VRMMeta>(l, r,
            (x, y) => x[0].Meta.Equals(y[0].Meta))
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
}
