using System;
using System.Linq;
using System.Collections.Generic;


namespace UniJSON
{
    public static class ArraySegmentExtensions
    {
        public static T[] ArrayOrCopy<T>(this ArraySegment<T> self)
        {
            if (self.Array == null || self.Count==0)
            {
                return new T[] { };
            }
            else if(self.Offset==0 && self.Count==self.Array.Length)
            {
                return self.Array;
            }
            else
            {
                var array = new T[self.Count];
                Array.Copy(self.Array, self.Offset, array, 0, self.Count);
                return array;
            }
        }

        public static IEnumerable<T> ToEnumerable<T>(this ArraySegment<T> self)
        {
            return self.Array.Skip(self.Offset).Take(self.Count);
        }

        public static void Set<T>(this ArraySegment<T> self, int index, T value)
        {
            if (index < 0 || index >= self.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            self.Array[self.Offset + index] = value;
        }

        public static T Get<T>(this ArraySegment<T> self, int index)
        {
            if (index < 0 || index >= self.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return self.Array[self.Offset + index];
        }

        public static ArraySegment<T> Advance<T>(this ArraySegment<T> self, Int32 n)
        {
            return new ArraySegment<T>(self.Array, self.Offset + n, self.Count - n);
        }

        public static ArraySegment<T> Take<T>(this ArraySegment<T> self, Int32 n)
        {
            return new ArraySegment<T>(self.Array, self.Offset, n);
        }

        public static T[] TakeReversedArray<T>(this ArraySegment<T> self, Int32 n)
        {
            var array = new T[n];
            var x = n - 1;
            for (int i = 0; i < n; ++i, --x)
            {
                array[i] = self.Get(x);
            }
            return array;
        }

        public static byte[] Concat(this byte[] lhs, ArraySegment<byte> rhs)
        {
            return new ArraySegment<byte>(lhs).Concat(rhs);
        }

        public static byte[] Concat(this ArraySegment<byte> lhs, ArraySegment<byte> rhs)
        {
            var bytes = new byte[lhs.Count + rhs.Count];
            Buffer.BlockCopy(lhs.Array, lhs.Offset, bytes, 0, lhs.Count);
            Buffer.BlockCopy(rhs.Array, rhs.Offset, bytes, lhs.Count, rhs.Count);
            return bytes;
        }
    }
}
