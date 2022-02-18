using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace UniGLTF
{
    public static class Pin
    {
        public static Pin<T> Create<T>(ArraySegment<T> src) where T : struct
        {
            return new Pin<T>(src);
        }
        public static Pin<T> Create<T>(T[] src) where T : struct
        {
            return Create(new ArraySegment<T>(src));
        }
    }
    public class Pin<T> : IDisposable
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

        public Pin(ArraySegment<T> src)
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

    public static class ArrayExtensions
    {

        public static T[] SelectInplace<T>(this T[] src, Func<T, T> pred)
        {
            for (int i = 0; i < src.Length; ++i)
            {
                src[i] = pred(src[i]);
            }
            return src;
        }
    }


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
