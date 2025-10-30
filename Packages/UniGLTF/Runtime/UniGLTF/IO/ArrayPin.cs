using System;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    public static class ArrayPin
    {
        public static ArrayPin<T> Create<T>(ArraySegment<T> src) where T : struct
        {
            return new ArrayPin<T>(src);
        }
        public static ArrayPin<T> Create<T>(T[] src) where T : struct
        {
            return Create(new ArraySegment<T>(src));
        }
    }

    public class ArrayPin<T> : IDisposable
        where T : struct
    {
        GCHandle m_pinnedArray;

        ArraySegment<T> m_src;

        public int Length
        {
            get
            {
                return m_src.Count * Marshal.SizeOf(typeof(T));
            }
        }

        public ArrayPin(ArraySegment<T> src)
        {
            m_src = src;
            m_pinnedArray = GCHandle.Alloc(src.Array, GCHandleType.Pinned);
        }

        public IntPtr Ptr
        {
            get
            {
                var ptr = m_pinnedArray.AddrOfPinnedObject();
                return new IntPtr(ptr.ToInt64() + m_src.Offset);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                if (m_pinnedArray.IsAllocated)
                {
                    m_pinnedArray.Free();
                }

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~Pin() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class ArrayPinExtensions
    {
        public static int FromBytes<T>(this ArraySegment<byte> src, T[] dst) where T : struct
        {
            SafeMarshalCopy.CopyBytesToArray(src, dst);
            return src.Count;
        }

        public static int ToBytes<T>(this T[] src, ArraySegment<byte> dst) where T : struct
        {
            SafeMarshalCopy.CopyArrayToToBytes(src, dst);
            return dst.Count;
        }
    }
}
