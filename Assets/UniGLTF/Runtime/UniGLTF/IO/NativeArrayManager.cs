using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using System.Runtime.InteropServices;

namespace UniGLTF
{
    /// <summary>
    /// NativeArrayManager を Dispose する責務を負わない使用者は、こっちを使う。
    /// </summary>
    public interface INativeArrayManager
    {
        NativeArray<T> CreateNativeArray<T>(int size)
        where T : struct;

        NativeArray<T> CreateNativeArray<T>(ArraySegment<T> data)
        where T : struct;

        NativeArray<T> CreateNativeArray<T>(T[] data)
        where T : struct;

        NativeArray<U> Convert<T, U>(NativeArray<T> src, Func<T, U> convert)
        where T : struct
        where U : struct;
    }

    /// <summary>
    /// 特定のコンテキスト(GltfDataなど)に関連する、NativeArrayの作成を代行し、
    /// まとめてDisposeできるようにする。
    /// 
    /// 例えば indexバッファー NativeArray<uint> の元が NativeArray<ushort> である場合に
    /// 新規に NativeArray を作成し Dispose 対象として管理する必要がある。
    /// また、Sparse や base64 encoding など単純なバイト列のスライスで済まない場合も同様である。
    /// 
    /// </summary>
    public class NativeArrayManager : INativeArrayManager, IDisposable
    {
        List<IDisposable> m_disposables = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var disposable in m_disposables)
            {
                disposable.Dispose();
            }
            m_disposables.Clear();
        }

        public NativeArrayManager()
        {

        }

        /// <summary>
        /// NativeArrayを新規作成し、Dispose管理する。
        /// 個別にDisposeする必要が無い。
        /// </summary>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public NativeArray<T> CreateNativeArray<T>(int size) where T : struct
        {
            var array = new NativeArray<T>(size, Allocator.Persistent);
            m_disposables.Add(array);
            return array;
        }

        public NativeArray<T> CreateNativeArray<T>(ArraySegment<T> data) where T : struct
        {
            var array = CreateNativeArray<T>(data.Count);
#if UNITY_2022_2_OR_NEWER
            var toSpan = array.AsSpan();
            var fromSpan = data.AsSpan();
            fromSpan.CopyTo(toSpan);
#else
            for (int i = 0; i < data.Count; i++)
                array[i] = data.Array[data.Offset + i];
#endif
            return array;
        }

        public NativeArray<T> CreateNativeArray<T>(T[] data) where T : struct
        {
            var array = CreateNativeArray<T>(data.Length);
            array.CopyFrom(data);
            return array;
        }

        /// <summary>
        /// サイズの違う型にコピーする。
        /// 
        /// 例
        /// NativeArray<ushort> => NativeArray<uint>
        /// </summary>
        public NativeArray<TDst> Convert<TSrc, TDst>(NativeArray<TSrc> src, Func<TSrc, TDst> convert)
        where TSrc : struct
        where TDst : struct
        {
            var dst = CreateNativeArray<TDst>(src.Length);
            for (var i = 0; i < src.Length; ++i)
            {
                dst[i] = convert(src[i]);
            }
            return dst;
        }
    }
}
