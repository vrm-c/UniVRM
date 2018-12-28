using System.Collections.Generic;
using UnityEngine;


namespace UniHumanoid
{
    public interface IBone
    {
        string Name { get; }
        Vector3 SkeletonLocalPosition { get; }
        IBone Parent { get; }
        IList<IBone> Children { get; }
    }

    public static class IBoneExtensions
    {
        public static IEnumerable<IBone> Traverse(this IBone self)
        {
            yield return self;
            foreach (var child in self.Children)
            {
                foreach (var x in child.Traverse())
                {
                    yield return x;
                }
            }
        }

        public static Vector3 CenterOfDescendant(this IBone self)
        {
            var sum = Vector3.zero;
            int i = 0;
            foreach (var x in self.Traverse())
            {
                sum += x.SkeletonLocalPosition;
                ++i;
            }
            return sum / i;
        }
    }
}
