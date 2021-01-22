using System;

namespace VrmLib
{
    public static class ArraySegmentExtensions
    {
        public static ArraySegment<T> Slice<T>(this ArraySegment<T> self, int start, int length)
        {
            if (start + length > self.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return new ArraySegment<T>(
                self.Array,
                self.Offset + start,
                length
            );
        }

        public static ArraySegment<T> Slice<T>(this ArraySegment<T> self, int start)
        {
            if (start > self.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return self.Slice(start, self.Count - start);
        }
    }
}
