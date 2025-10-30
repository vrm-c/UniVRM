using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    /// <summary>
    /// Marshal.Copy
    /// * ptr to bytes 
    /// * bytes to ptr
    /// の両方向がある。
    /// ptr になったら範囲は分からん。
    /// ptr にする前に範囲チェックするのを明確にするのがこの Utility の意図である。
    /// 
    /// Marshal.Copy を使わずにこの関数を使うべし
    /// 
    /// </summary>
    public static class SafeMarshalCopy
    {
        /// <summary>
        /// bytes から T[] へのコピー
        /// </summary>
        public static void CopyBytesToArray<T>(ArraySegment<byte> src, T[] dst) where T : struct
        {
            if (src.Array == null || dst == null || src.Count == 0)
            {
                throw new System.ArgumentNullException();
            }
            if (src.Offset < 0)
            {
                throw new System.AccessViolationException("CopyBytesToArray: ArraySegment: negative offset");
            }
            if ((src.Offset + src.Count) > src.Array.Length)
            {
                throw new System.AccessViolationException("CopyBytesToArray: ArraySegment: exceed");
            }
            var dstByteSize = dst.Length * Marshal.SizeOf(typeof(T));
            if (src.Count > dstByteSize)
            {
                throw new System.AccessViolationException("CopyBytesToArray: src > dst");
            }

            using (var pin = Pin.Create(dst))
            {
                Marshal.Copy(src.Array, src.Offset, pin.Ptr, src.Count);
            }
        }

        /// <summary>
        /// T[] から bytes へのコピー
        /// </summary>
        public static void CopyArrayToToBytes<T>(T[] src, ArraySegment<byte> dst) where T : struct
        {
            if (dst.Array == null || src == null || dst.Count == 0)
            {
                throw new System.ArgumentNullException();
            }
            if (dst.Offset < 0)
            {
                throw new System.AccessViolationException("CopyArrayToToBytes: ArraySegment: negative offset");
            }
            if (dst.Offset + dst.Count > dst.Array.Length)
            {
                throw new System.AccessViolationException("CopyArrayToToBytes: ArraySegment: exceed");
            }
            var srcByteSize = src.Length * Marshal.SizeOf(typeof(T));
            if (srcByteSize > dst.Count)
            {
                throw new System.AccessViolationException("CopyArrayToToBytes: src > dst");
            }

            using (var pin = Pin.Create(src))
            {
                Marshal.Copy(pin.Ptr, dst.Array, dst.Offset, srcByteSize);
            }
        }
    }
}
