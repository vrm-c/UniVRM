using System;
using System.Linq;
using System.Collections.Generic;


public static class TriangleUtil
{
    public static IEnumerable<int> FlipTriangle(IEnumerable<Byte> src)
    {
        return FlipTriangle(src.Select(x => (Int32)x));
    }

    public static IEnumerable<int> FlipTriangle(IEnumerable<UInt16> src)
    {
        return FlipTriangle(src.Select(x => (Int32)x));
    }

    public static IEnumerable<int> FlipTriangle(IEnumerable<UInt32> src)
    {
        return FlipTriangle(src.Select(x => (Int32)x));
    }

    public static IEnumerable<int> FlipTriangle(IEnumerable<Int32> src)
    {
        var it = src.GetEnumerator();
        while (true)
        {
            if (!it.MoveNext())
            {
                yield break;
            }
            var i0 = it.Current;

            if (!it.MoveNext())
            {
                yield break;
            }
            var i1 = it.Current;

            if (!it.MoveNext())
            {
                yield break;
            }
            var i2 = it.Current;

            yield return i2;
            yield return i1;
            yield return i0;
        }
    }
}
